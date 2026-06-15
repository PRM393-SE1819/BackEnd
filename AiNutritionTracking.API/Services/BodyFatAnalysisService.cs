using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.AI;
using AiNutritionTracking.API.Helpers;
using AiNutritionTracking.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiNutritionTracking.API.Services;

public class BodyFatAnalysisService : IBodyFatAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly GroqSettings _groqSettings;
    private readonly AinutritiontrackingContext _db;
    private readonly ILogger<BodyFatAnalysisService> _logger;

    public BodyFatAnalysisService(
        HttpClient httpClient,
        IOptions<GroqSettings> groqSettings,
        AinutritiontrackingContext db,
        ILogger<BodyFatAnalysisService> logger)
    {
        _httpClient   = httpClient;
        _groqSettings = groqSettings.Value;
        _db           = db;
        _logger       = logger;
    }

    // ── US Navy Formula ───────────────────────────────────────────────────────

    private static double CalcNavyBodyFat(string gender, double height, double waist, double neck, double? hip)
    {
        if (string.Equals(gender, "Female", StringComparison.OrdinalIgnoreCase) && hip.HasValue)
        {
            return 163.205 * Math.Log10(waist + hip.Value - neck)
                   - 97.684 * Math.Log10(height)
                   - 78.387;
        }

        return 86.010 * Math.Log10(waist - neck)
               - 70.041 * Math.Log10(height)
               + 36.76;
    }

    // ── Body fat category ─────────────────────────────────────────────────────

    private static string GetCategory(string gender, double bodyFat)
    {
        bool isFemale = string.Equals(gender, "Female", StringComparison.OrdinalIgnoreCase);

        if (isFemale)
        {
            return bodyFat switch
            {
                <= 13 => "Essential Fat",
                <= 20 => "Athlete",
                <= 24 => "Fitness",
                <= 31 => "Average",
                _     => "Obese"
            };
        }

        return bodyFat switch
        {
            <= 5  => "Essential Fat",
            <= 13 => "Athlete",
            <= 17 => "Fitness",
            <= 24 => "Average",
            _     => "Obese"
        };
    }

    private static string GetHealthAssessment(string category)
    {
        return category switch
        {
            "Essential Fat" => "Critically low body fat — minimum required for vital organ function.",
            "Athlete"       => "Excellent athletic body composition.",
            "Fitness"       => "Healthy body composition.",
            "Average"       => "Acceptable body fat within normal population range.",
            "Obese"         => "Elevated body fat — associated with increased health risks.",
            _               => "Body composition assessed."
        };
    }

    private static double? CalcTargetWeight(double weight, double bodyFat, string gender)
    {
        bool isFemale = string.Equals(gender, "Female", StringComparison.OrdinalIgnoreCase);
        double targetBodyFat = isFemale ? 22.0 : 15.5; // midpoint of Fitness range

        if (bodyFat <= targetBodyFat)
            return null; // already at or below fitness target

        double leanMass = weight * (1 - bodyFat / 100.0);
        double target   = leanMass / (1 - targetBodyFat / 100.0);
        return Math.Round(target, 1);
    }

    // ── Groq API ──────────────────────────────────────────────────────────────

    private async Task<string> CallGroqVisionAsync(string base64Image, string mimeType,
        string gender, int age, double height, double weight, int imageIndex, int totalImages)
    {
        var systemPrompt =
            "You are a professional fitness analyst and body composition expert. " +
            "Analyze the provided body photo and return ONLY a valid JSON object — no markdown, no extra text. " +
            "Schema: {\"estimatedBodyFat\":0.0,\"muscleDef\":\"string\",\"fatDistribution\":\"string\",\"recommendation\":\"string\"}. " +
            "estimatedBodyFat is a number (percentage). muscleDef describes muscle definition. fatDistribution describes where fat is stored.";

        var dataUrl  = $"data:{mimeType};base64,{base64Image}";
        var userText =
            $"Patient: {gender}, {age} years old, height {height} cm, weight {weight} kg. " +
            $"This is photo {imageIndex} of {totalImages}. Estimate body fat percentage from this body photo.";

        var messages = new object[]
        {
            new { role = "system", content = systemPrompt },
            new
            {
                role    = "user",
                content = new object[]
                {
                    new { type = "image_url", image_url = new { url = dataUrl } },
                    new { type = "text",      text      = userText }
                }
            }
        };

        var body = new
        {
            model       = _groqSettings.VisionModel,
            messages,
            temperature = 0.3,
            max_tokens  = 1024
        };

        var json        = JsonSerializer.Serialize(body);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqSettings.ApiKey}");

        var response = await _httpClient.PostAsync($"{_groqSettings.BaseUrl}/chat/completions", httpContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Groq API error {Status}: {Body}", (int)response.StatusCode, errorBody);
            throw new InvalidOperationException($"Groq {(int)response.StatusCode}: {errorBody}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc    = JsonDocument.Parse(responseJson);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }

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
        var start = text.IndexOf('{');
        return start >= 0 ? text[start..] : text;
    }

    // ── Persist ───────────────────────────────────────────────────────────────

    private async Task PersistAsync(int userId, double bodyFat, string category,
        string healthAssessment, string recommendation, double? targetWeight)
    {
        var record = new BodyFatAnalysis
        {
            UserId          = userId,
            EstimatedBodyFat = Math.Round(bodyFat, 1),
            Category        = category,
            HealthAssessment = healthAssessment,
            Recommendation  = recommendation,
            TargetWeight    = targetWeight,
            CreatedAt       = DateTime.UtcNow
        };
        _db.BodyFatAnalyses.Add(record);
        await _db.SaveChangesAsync();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public async Task<BodyFatAnalysisResponseDto> AnalyzeFromImagesAsync(BodyFatImageRequestDto request, int userId)
    {
        try
        {
            var images = request.Images;
            var total  = images.Count;
            var bodyFatReadings      = new List<double>();
            var recommendationTexts  = new List<string>();

            for (int i = 0; i < total; i++)
            {
                var image = images[i];
                string base64;
                using (var ms = new MemoryStream())
                {
                    await image.CopyToAsync(ms);
                    base64 = Convert.ToBase64String(ms.ToArray());
                }
                var mimeType = image.ContentType ?? "image/jpeg";

                var rawContent = await CallGroqVisionAsync(
                    base64, mimeType,
                    request.Gender, request.Age, request.Height, request.Weight,
                    i + 1, total);

                var parsed = JsonSerializer.Deserialize<JsonElement>(ExtractJson(rawContent),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed.TryGetProperty("estimatedBodyFat", out var bf))
                    bodyFatReadings.Add(bf.GetDouble());

                if (parsed.TryGetProperty("recommendation", out var rec) && rec.GetString() is { Length: > 0 } r)
                    recommendationTexts.Add(r);
            }

            var bodyFat      = bodyFatReadings.Count > 0 ? Math.Round(bodyFatReadings.Average(), 1) : 20.0;
            var recommendation = recommendationTexts.FirstOrDefault() ?? string.Empty;

            var category     = GetCategory(request.Gender, bodyFat);
            var assessment   = GetHealthAssessment(category);
            var targetWeight = CalcTargetWeight(request.Weight, bodyFat, request.Gender);

            await PersistAsync(userId, bodyFat, category, assessment, recommendation, targetWeight);

            return new BodyFatAnalysisResponseDto
            {
                EstimatedBodyFat = bodyFat,
                Category         = category,
                HealthAssessment = assessment,
                Recommendation   = recommendation,
                TargetWeight     = targetWeight
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Body fat vision analysis failed for userId={UserId}", userId);
            throw;
        }
    }

    public async Task<BodyFatAnalysisResponseDto> AnalyzeFromMeasurementsAsync(BodyFatMeasurementRequestDto request, int userId)
    {
        try
        {
            var bodyFat = CalcNavyBodyFat(request.Gender, request.Height, request.Waist, request.Neck, request.Hip);
            bodyFat     = Math.Max(2, Math.Round(bodyFat, 1));

            var category     = GetCategory(request.Gender, bodyFat);
            var assessment   = GetHealthAssessment(category);
            var targetWeight = CalcTargetWeight(request.Weight, bodyFat, request.Gender);

            var recommendation = category switch
            {
                "Essential Fat" => "Your body fat is critically low. Consult a healthcare professional immediately.",
                "Athlete"       => "Excellent body composition. Maintain your current training and nutrition.",
                "Fitness"       => $"Healthy body composition. Reduce body fat by 2-3% for peak athletic performance.",
                "Average"       => $"Aim to reduce body fat by 5-8% through a caloric deficit and resistance training.",
                "Obese"         => $"Significant body fat reduction needed. A structured diet and exercise program is strongly recommended.",
                _               => "Maintain a balanced diet and regular exercise."
            };

            await PersistAsync(userId, bodyFat, category, assessment, recommendation, targetWeight);

            return new BodyFatAnalysisResponseDto
            {
                EstimatedBodyFat = bodyFat,
                Category         = category,
                HealthAssessment = assessment,
                Recommendation   = recommendation,
                TargetWeight     = targetWeight
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Body fat formula analysis failed for userId={UserId}", userId);
            throw;
        }
    }

    // ── History ───────────────────────────────────────────────────────────────

    public async Task<List<BodyFatHistoryDto>> GetHistoryAsync(int userId)
    {
        return await _db.BodyFatAnalyses
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BodyFatHistoryDto
            {
                Id               = x.Id,
                EstimatedBodyFat = x.EstimatedBodyFat,
                Category         = x.Category,
                HealthAssessment = x.HealthAssessment,
                Recommendation   = x.Recommendation,
                TargetWeight     = x.TargetWeight,
                CreatedAt        = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<BodyFatHistoryDto?> GetHistoryByIdAsync(int id, int userId)
    {
        return await _db.BodyFatAnalyses
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new BodyFatHistoryDto
            {
                Id               = x.Id,
                EstimatedBodyFat = x.EstimatedBodyFat,
                Category         = x.Category,
                HealthAssessment = x.HealthAssessment,
                Recommendation   = x.Recommendation,
                TargetWeight     = x.TargetWeight,
                CreatedAt        = x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteHistoryAsync(int id, int userId)
    {
        var record = await _db.BodyFatAnalyses
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (record is null) return false;

        _db.BodyFatAnalyses.Remove(record);
        await _db.SaveChangesAsync();
        return true;
    }
}
