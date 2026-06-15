using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AiNutritionTracking.API.DTOs.AI;
using AiNutritionTracking.API.Helpers;
using AiNutritionTracking.API.Repositories;
using Microsoft.Extensions.Options;

namespace AiNutritionTracking.API.Services;

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly GroqSettings _groqSettings;
    private readonly IAIRepository _aiRepository;
    private readonly IPexelsService _pexels;
    private readonly ILogger<AIService> _logger;

    private const double CalorieWarnThreshold = 900;
    private const double ProteinWarnThreshold = 60;
    private const double CarbsWarnThreshold   = 120;
    private const double FatWarnThreshold     = 40;

    public AIService(
        HttpClient httpClient,
        IOptions<GroqSettings> groqSettings,
        IAIRepository aiRepository,
        IPexelsService pexels,
        ILogger<AIService> logger)
    {
        _httpClient   = httpClient;
        _groqSettings = groqSettings.Value;
        _aiRepository = aiRepository;
        _pexels       = pexels;
        _logger       = logger;
    }

    // ── Core Groq call ───────────────────────────────────────────────────────

    private async Task<(string content, int tokensUsed, int responseTimeMs)> CallGroqAsync(
        string userPrompt,
        string systemPrompt,
        string? model = null,
        object[]? messages = null)
    {
        var sw = Stopwatch.StartNew();

        var requestMessages = messages ?? new object[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user",   content = userPrompt   }
        };

        var body = new
        {
            model       = model ?? _groqSettings.Model,
            messages    = requestMessages,
            temperature = 0.3,
            max_tokens  = 4096
        };

        var json        = JsonSerializer.Serialize(body);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqSettings.ApiKey}");

        var response = await _httpClient.PostAsync($"{_groqSettings.BaseUrl}/chat/completions", httpContent);
        response.EnsureSuccessStatusCode();
        sw.Stop();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc    = JsonDocument.Parse(responseJson);
        var root         = doc.RootElement;

        var content = root
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;

        var tokensUsed = root.TryGetProperty("usage", out var usage) &&
                         usage.TryGetProperty("total_tokens", out var tok)
            ? tok.GetInt32() : 0;

        return (content, tokensUsed, (int)sw.ElapsedMilliseconds);
    }

    // ── JSON helpers ─────────────────────────────────────────────────────────

    private static string ExtractJson(string raw)
    {
        var text = raw.Trim();
        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            var lastFence    = text.LastIndexOf("```");
            if (firstNewline > 0 && lastFence > firstNewline)
                text = text[(firstNewline + 1)..lastFence].Trim();
        }
        var objStart = text.IndexOf('{');
        var arrStart = text.IndexOf('[');
        int start    = (objStart, arrStart) switch
        {
            ( >= 0, >= 0) => Math.Min(objStart, arrStart),
            ( >= 0, _)    => objStart,
            (_, >= 0)     => arrStart,
            _             => 0
        };
        return text[start..];
    }

    private T? ParseJson<T>(string raw) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(ExtractJson(raw),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JSON parse failed. Raw: {Raw}", raw);
            return null;
        }
    }

    // ── Pexels image attachment helpers ──────────────────────────────────────

    private async Task AttachImageAsync(MealItemDto meal)
    {
        if (!string.IsNullOrWhiteSpace(meal.Name))
            meal.ImageUrl = await _pexels.SearchFoodImageAsync(meal.Name);
    }

    // ── Nutrition warnings ────────────────────────────────────────────────────

    private static List<string> BuildNutritionWarnings(double cal, double pro, double carbs, double fat)
    {
        var w = new List<string>();
        if (cal   > CalorieWarnThreshold) w.Add($"⚠️ HIGH CALORIES: {cal} kcal exceeds {CalorieWarnThreshold} kcal per meal.");
        if (pro   > ProteinWarnThreshold) w.Add($"⚠️ HIGH PROTEIN: {pro}g per meal may strain kidneys.");
        if (carbs > CarbsWarnThreshold)   w.Add($"⚠️ HIGH CARBS: {carbs}g per meal may spike blood sugar.");
        if (fat   > FatWarnThreshold)     w.Add($"⚠️ HIGH FAT: {fat}g per meal increases cardiovascular risk.");
        return w;
    }

    // ── 1. Chat ───────────────────────────────────────────────────────────────

    public async Task<ChatResponseDto> ChatAsync(string message, int userId)
    {
        var db = await _aiRepository.SaveRequestAsync(userId, "Chat", message, "Groq", _groqSettings.Model);
        try
        {
            const string sys =
                "You are a professional nutrition coach with expertise in dietetics, sports nutrition, " +
                "and healthy eating. Answer questions about nutrition, diet, macronutrients, micronutrients, " +
                "meal planning, food allergies, and general wellness clearly and with evidence-based advice. " +
                "Always recommend consulting a healthcare professional for medical concerns.";
            var (content, tokens, ms) = await CallGroqAsync(message, sys);
            await _aiRepository.SaveResponseAsync(db.RequestId, content, tokens, ms);
            return new ChatResponseDto { Success = true, Answer = content };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat error userId={UserId}", userId);
            await _aiRepository.MarkRequestFailedAsync(db.RequestId, ex.Message);
            return new ChatResponseDto { Success = false, Answer = "Sorry, I couldn't process your request right now." };
        }
    }

    // ── 2. Food Image Analysis ────────────────────────────────────────────────

    public async Task<FoodImageAnalysisResponseDto> AnalyzeImageAsync(IFormFile image, int userId)
    {
        var db = await _aiRepository.SaveRequestAsync(userId, "ImageAnalysis", image.FileName, "Groq", _groqSettings.VisionModel);
        try
        {
            string base64Image;
            using (var ms = new MemoryStream())
            {
                await image.CopyToAsync(ms);
                base64Image = Convert.ToBase64String(ms.ToArray());
            }
            var mimeType = image.ContentType ?? "image/jpeg";
            var dataUrl  = $"data:{mimeType};base64,{base64Image}";

            const string sys =
                "You are a professional food analyst and registered dietitian. " +
                "Analyze the food in the image and return ONLY a valid JSON object — no markdown, no extra text. " +
                "Schema: {\"foodName\":\"string\",\"estimatedCalories\":0.0,\"protein\":0.0,\"carbs\":0.0,\"fat\":0.0,\"description\":\"string\"}.";

            var visionMessages = new object[]
            {
                new { role = "system", content = sys },
                new
                {
                    role    = "user",
                    content = new object[]
                    {
                        new { type = "text",      text      = "Analyze this food image and return the JSON nutritional breakdown." },
                        new { type = "image_url", image_url = new { url = dataUrl } }
                    }
                }
            };

            (string content, int tokens, int ms2) result;
            try
            {
                result = await CallGroqAsync(string.Empty, sys, _groqSettings.VisionModel, visionMessages);
            }
            catch (Exception visionEx)
            {
                _logger.LogWarning(visionEx, "Vision model failed, falling back for {File}", image.FileName);
                const string fallbackSys =
                    "You are a nutrition estimator. Return ONLY a valid JSON object — no markdown, no extra text. " +
                    "Schema: {\"foodName\":\"string\",\"estimatedCalories\":0.0,\"protein\":0.0,\"carbs\":0.0,\"fat\":0.0,\"description\":\"string\"}.";
                var fallbackPrompt = $"Estimate nutrition for: {image.FileName.Replace("_", " ").Replace("-", " ")}";
                result = await CallGroqAsync(fallbackPrompt, fallbackSys);
            }

            await _aiRepository.SaveResponseAsync(db.RequestId, result.content, result.tokens, result.ms2);
            var analysis = ParseJson<FoodImageAnalysisResponseDto>(result.content) ?? new FoodImageAnalysisResponseDto();
            analysis.Warnings = BuildNutritionWarnings(analysis.EstimatedCalories, analysis.Protein, analysis.Carbs, analysis.Fat);
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image analysis error userId={UserId}", userId);
            await _aiRepository.MarkRequestFailedAsync(db.RequestId, ex.Message);
            return new FoodImageAnalysisResponseDto
            {
                FoodName    = "Unknown",
                Description = "Could not analyze the image. Please ensure the image is clear and try again.",
                Warnings    = new List<string> { "Analysis failed. Please try again with a clearer image." }
            };
        }
    }

    // ── 3. Calorie Estimate ───────────────────────────────────────────────────

    public async Task<CalorieEstimateResponseDto> EstimateCaloriesAsync(string foodDescription, int userId)
    {
        var db = await _aiRepository.SaveRequestAsync(userId, "CalorieEstimate", foodDescription, "Groq", _groqSettings.Model);
        try
        {
            const string sys =
                "You are a precision nutrition calculator. Return ONLY a valid JSON object — no markdown, no extra text. " +
                "Schema: {\"calories\":0.0,\"protein\":0.0,\"carbs\":0.0,\"fat\":0.0}.";
            var (content, t, ms)  = await CallGroqAsync($"Calculate nutrition for: {foodDescription}", sys);
            await _aiRepository.SaveResponseAsync(db.RequestId, content, t, ms);
            return ParseJson<CalorieEstimateResponseDto>(content) ?? new CalorieEstimateResponseDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CalorieEstimate error userId={UserId}", userId);
            await _aiRepository.MarkRequestFailedAsync(db.RequestId, ex.Message);
            return new CalorieEstimateResponseDto();
        }
    }

    // ── 4. Meal Recommendation with Images ────────────────────────────────────

    public async Task<MealRecommendationResponseDto> GetMealRecommendationsAsync(MealRecommendationRequestDto request, int userId)
    {
        var prompt =
            $"Create a healthy single-day meal plan for: {request.Age}yo {request.Gender}, " +
            $"{request.Weight}kg, {request.Height}cm, activity={request.ActivityLevel}, goal={request.Goal}.";
        var db = await _aiRepository.SaveRequestAsync(userId, "MealRecommendation", prompt, "Groq", _groqSettings.Model);
        try
        {
            var sys =
                "You are a certified dietitian. Return ONLY a valid JSON object — no markdown, no extra text.\n" +
                "Use exactly this schema:\n" +
                "{\n" +
                "  \"breakfast\":{\"name\":\"...\",\"description\":\"...\",\"calories\":0,\"protein\":0.0,\"carbs\":0.0,\"fat\":0.0},\n" +
                "  \"lunch\":    {\"name\":\"...\",\"description\":\"...\",\"calories\":0,\"protein\":0.0,\"carbs\":0.0,\"fat\":0.0},\n" +
                "  \"dinner\":   {\"name\":\"...\",\"description\":\"...\",\"calories\":0,\"protein\":0.0,\"carbs\":0.0,\"fat\":0.0},\n" +
                "  \"snack\":    {\"name\":\"...\",\"description\":\"...\",\"calories\":0,\"protein\":0.0,\"carbs\":0.0,\"fat\":0.0},\n" +
                "  \"totalCalories\":0,\n" +
                "  \"notes\":\"...\"\n" +
                "}";

            var (content, t, ms) = await CallGroqAsync(prompt, sys);
            await _aiRepository.SaveResponseAsync(db.RequestId, content, t, ms);
            _logger.LogInformation("Recommendation raw: {Raw}", content);

            var result = ParseJson<MealRecommendationResponseDto>(content) ?? new MealRecommendationResponseDto();

            // Attach food images in parallel
            await Task.WhenAll(
                AttachImageAsync(result.Breakfast),
                AttachImageAsync(result.Lunch),
                AttachImageAsync(result.Dinner),
                AttachImageAsync(result.Snack)
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Recommendation error userId={UserId}", userId);
            await _aiRepository.MarkRequestFailedAsync(db.RequestId, ex.Message);
            return new MealRecommendationResponseDto();
        }
    }

    // ── 5. 7-Day Meal Plan with Images ────────────────────────────────────────

    public async Task<MealPlanResponseDto> GetMealPlanAsync(MealPlanRequestDto request, int userId)
    {
        var prompt = $"Goal: {request.Goal}. Daily calorie target: {request.DailyCalories} kcal.";
        var db     = await _aiRepository.SaveRequestAsync(userId, "MealPlan", prompt, "Groq", _groqSettings.Model);
        try
        {
            var sys =
                "You are a professional meal planner. Generate a complete 7-day meal plan.\n" +
                "Return ONLY a valid JSON object — no markdown, no extra text, no code fences.\n" +
                "Use exactly this schema (repeat the day object for all 7 days Monday-Sunday):\n" +
                "{\n" +
                "  \"days\": [\n" +
                "    {\n" +
                "      \"day\": \"Monday\",\n" +
                "      \"breakfast\": {\"name\":\"...\",\"description\":\"...\",\"calories\":0,\"protein\":0.0,\"carbs\":0.0,\"fat\":0.0},\n" +
                "      \"lunch\":     {\"name\":\"...\",\"description\":\"...\",\"calories\":0,\"protein\":0.0,\"carbs\":0.0,\"fat\":0.0},\n" +
                "      \"dinner\":    {\"name\":\"...\",\"description\":\"...\",\"calories\":0,\"protein\":0.0,\"carbs\":0.0,\"fat\":0.0},\n" +
                "      \"snack\":     {\"name\":\"...\",\"description\":\"...\",\"calories\":0,\"protein\":0.0,\"carbs\":0.0,\"fat\":0.0},\n" +
                "      \"totalCalories\": 0\n" +
                "    }\n" +
                "  ]\n" +
                "}";

            var (content, t, ms) = await CallGroqAsync(prompt, sys);
            await _aiRepository.SaveResponseAsync(db.RequestId, content, t, ms);
            _logger.LogInformation("MealPlan raw: {Raw}", content);

            var result = ParseJson<MealPlanResponseDto>(content);
            if (result == null || result.Days.Count == 0)
            {
                _logger.LogWarning("MealPlan parse empty. Raw: {Raw}", content);
                return new MealPlanResponseDto();
            }

            // Attach food images for all meals across all days in parallel
            var imageTasks = result.Days.SelectMany(day => new[]
            {
                AttachImageAsync(day.Breakfast),
                AttachImageAsync(day.Lunch),
                AttachImageAsync(day.Dinner),
                AttachImageAsync(day.Snack)
            });
            await Task.WhenAll(imageTasks);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MealPlan error userId={UserId}", userId);
            await _aiRepository.MarkRequestFailedAsync(db.RequestId, ex.Message);
            return new MealPlanResponseDto();
        }
    }

    // ── Chat History ──────────────────────────────────────────────────────────

    public async Task<List<ChatHistoryDto>> GetChatHistoryAsync(int userId, int page, int pageSize)
    {
        var records = await _aiRepository.GetChatHistoryAsync(userId, page, pageSize);

        return records.Select(r => new ChatHistoryDto
        {
            RequestId     = r.RequestId,
            Question      = r.Prompt ?? string.Empty,
            Answer        = r.Airesponses.FirstOrDefault()?.RawResponse ?? string.Empty,
            TokensUsed    = r.TokensUsed,
            ResponseTimeMs = r.ResponseTimeMs,
            Status        = r.Status ?? string.Empty,
            RequestedAt   = r.RequestedAt
        }).ToList();
    }
}
