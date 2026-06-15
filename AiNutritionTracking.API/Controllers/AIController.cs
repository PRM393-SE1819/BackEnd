using AiNutritionTracking.API.DTOs.AI;
using AiNutritionTracking.API.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AiNutritionTracking.API.Controllers;

/// <summary>
/// AI-powered nutrition endpoints — chatbot, image analysis, calorie estimation, meal recommendations, and weekly meal planning.
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
[EnableRateLimiting("ai-policy")]
[Produces("application/json")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly IValidator<ChatRequestDto> _chatValidator;
    private readonly IValidator<CalorieEstimateRequestDto> _calorieValidator;
    private readonly IValidator<MealRecommendationRequestDto> _recommendationValidator;
    private readonly IValidator<MealPlanRequestDto> _mealPlanValidator;
    private readonly ILogger<AIController> _logger;

    public AIController(
        IAIService aiService,
        IValidator<ChatRequestDto> chatValidator,
        IValidator<CalorieEstimateRequestDto> calorieValidator,
        IValidator<MealRecommendationRequestDto> recommendationValidator,
        IValidator<MealPlanRequestDto> mealPlanValidator,
        ILogger<AIController> logger)
    {
        _aiService               = aiService;
        _chatValidator           = chatValidator;
        _calorieValidator        = calorieValidator;
        _recommendationValidator = recommendationValidator;
        _mealPlanValidator       = mealPlanValidator;
        _logger                  = logger;
    }

    private int GetUserId()
    {
        var claim = User.FindFirst("id")?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    private IActionResult ValidationFailed(FluentValidation.Results.ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        return BadRequest(new { success = false, errors });
    }

    // ─── 1. AI Chatbot ───────────────────────────────────────────────────────

    /// <summary>Ask the AI nutrition coach a question.</summary>
    /// <remarks>
    /// Send any nutrition-related question and receive an expert AI answer.
    ///
    ///     POST /api/ai/chat
    ///     { "message": "How much protein should I eat daily?" }
    ///
    /// </remarks>
    /// <response code="200">AI response returned successfully.</response>
    /// <response code="400">Validation error — message is empty or too long.</response>
    /// <response code="429">Rate limit exceeded — max 10 requests per minute.</response>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
    {
        var validation = await _chatValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationFailed(validation);

        var userId = GetUserId();
        _logger.LogInformation("AI Chat request from userId={UserId}", userId);

        var response = await _aiService.ChatAsync(request.Message, userId);
        return Ok(response);
    }

    // ─── 2. Food Image Analysis ──────────────────────────────────────────────

    /// <summary>Analyze a food image to estimate nutritional content.</summary>
    /// <remarks>
    /// Upload a food photo and receive calories, macros, and health warnings if values exceed safe per-meal thresholds.
    ///
    ///     POST /api/ai/analyze-image
    ///     Content-Type: multipart/form-data
    ///     image: &lt;file&gt;
    ///
    /// Warnings are issued when:
    /// - Calories &gt; 900 kcal per meal
    /// - Protein &gt; 60 g per meal
    /// - Carbohydrates &gt; 120 g per meal
    /// - Fat &gt; 40 g per meal
    /// </remarks>
    /// <response code="200">Analysis result with optional nutritional warnings.</response>
    /// <response code="400">No image provided or unsupported file type.</response>
    /// <response code="429">Rate limit exceeded.</response>
    [HttpPost("analyze-image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FoodImageAnalysisResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AnalyzeImage([FromForm] AnalyzeImageRequestDto request)
    {
        var image = request.Image;
        if (image == null || image.Length == 0)
            return BadRequest(new { success = false, message = "Please upload a valid image file." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(image.ContentType?.ToLower()))
            return BadRequest(new { success = false, message = "Only JPEG, PNG, WEBP, or GIF images are accepted." });

        if (image.Length > 10 * 1024 * 1024)
            return BadRequest(new { success = false, message = "Image size must not exceed 10 MB." });

        var userId = GetUserId();
        _logger.LogInformation("AI Image Analysis request from userId={UserId}, file={FileName}", userId, image.FileName);

        var response = await _aiService.AnalyzeImageAsync(image, userId);
        return Ok(response);
    }

    // ─── 3. Calorie Estimation ───────────────────────────────────────────────

    /// <summary>Estimate nutrition facts from a text food description.</summary>
    /// <remarks>
    /// Describe your meal in plain text and receive an estimated nutritional breakdown.
    ///
    ///     POST /api/ai/calorie-estimate
    ///     { "foodDescription": "2 scrambled eggs and 2 slices of whole wheat bread" }
    ///
    /// </remarks>
    /// <response code="200">Estimated nutrition breakdown returned.</response>
    /// <response code="400">Validation error — description is empty or too long.</response>
    /// <response code="429">Rate limit exceeded.</response>
    [HttpPost("calorie-estimate")]
    [ProducesResponseType(typeof(CalorieEstimateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> EstimateCalories([FromBody] CalorieEstimateRequestDto request)
    {
        var validation = await _calorieValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationFailed(validation);

        var userId = GetUserId();
        _logger.LogInformation("AI Calorie Estimate from userId={UserId}", userId);

        var response = await _aiService.EstimateCaloriesAsync(request.FoodDescription, userId);
        return Ok(response);
    }

    // ─── 4. Meal Recommendation ──────────────────────────────────────────────

    /// <summary>Generate a personalized single-day meal recommendation.</summary>
    /// <remarks>
    /// Provide your profile and goal to receive a fully tailored daily meal plan with macros.
    ///
    ///     POST /api/ai/recommendation
    ///     {
    ///       "goal": "Weight Loss",
    ///       "age": 22,
    ///       "gender": "Male",
    ///       "weight": 75,
    ///       "height": 175,
    ///       "activityLevel": "Moderate"
    ///     }
    ///
    /// Valid activity levels: Sedentary, Light, Moderate, Active, Very Active
    /// </remarks>
    /// <response code="200">Daily meal recommendation returned.</response>
    /// <response code="400">Validation error in request.</response>
    /// <response code="429">Rate limit exceeded.</response>
    [HttpPost("recommendation")]
    [ProducesResponseType(typeof(MealRecommendationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetRecommendations([FromBody] MealRecommendationRequestDto request)
    {
        var validation = await _recommendationValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationFailed(validation);

        var userId = GetUserId();
        _logger.LogInformation("AI Meal Recommendation for userId={UserId}, goal={Goal}", userId, request.Goal);

        var response = await _aiService.GetMealRecommendationsAsync(request, userId);
        return Ok(response);
    }

    // ─── 5. 7-Day Meal Plan ──────────────────────────────────────────────────

    /// <summary>Generate a complete 7-day meal plan.</summary>
    /// <remarks>
    /// Provide your goal and daily calorie target to receive a full week of meals with snacks and macros for every day.
    ///
    ///     POST /api/ai/meal-plan
    ///     {
    ///       "goal": "Muscle Gain",
    ///       "dailyCalories": 2800
    ///     }
    ///
    /// Response includes breakfast, lunch, dinner, and snack for Monday through Sunday,
    /// each with name, description, calories, protein, carbs, and fat.
    /// </remarks>
    /// <response code="200">Full 7-day meal plan returned.</response>
    /// <response code="400">Validation error — invalid calories or missing goal.</response>
    /// <response code="429">Rate limit exceeded.</response>
    [HttpPost("meal-plan")]
    [ProducesResponseType(typeof(MealPlanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetMealPlan([FromBody] MealPlanRequestDto request)
    {
        var validation = await _mealPlanValidator.ValidateAsync(request);
        if (!validation.IsValid) return ValidationFailed(validation);

        var userId = GetUserId();
        _logger.LogInformation("AI Meal Plan for userId={UserId}, goal={Goal}, calories={Cal}", userId, request.Goal, request.DailyCalories);

        var response = await _aiService.GetMealPlanAsync(request, userId);
        return Ok(response);
    }

    // ─── 6. Chat History ─────────────────────────────────────────────────────

    /// <summary>Get chat history for the current user.</summary>
    /// <remarks>
    /// Returns a paginated list of past AI chat conversations, newest first.
    ///
    ///     GET /api/ai/chat/history?page=1&amp;pageSize=20
    ///
    /// </remarks>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="pageSize">Records per page, max 50 (default 20).</param>
    /// <response code="200">Paginated chat history.</response>
    [HttpGet("chat/history")]
    [ProducesResponseType(typeof(List<ChatHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChatHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 20;

        var userId = GetUserId();
        _logger.LogInformation("Chat history request userId={UserId} page={Page}", userId, page);

        var history = await _aiService.GetChatHistoryAsync(userId, page, pageSize);
        return Ok(new { page, pageSize, count = history.Count, data = history });
    }

    // ─── 7. Delete single chat record ────────────────────────────────────────

    /// <summary>Delete a specific chat record by ID.</summary>
    /// <response code="200">Deleted successfully.</response>
    /// <response code="404">Record not found or does not belong to the current user.</response>
    [HttpDelete("chat/history/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteChatRecord(int id)
    {
        var userId  = GetUserId();
        var deleted = await _aiService.DeleteChatRecordAsync(id, userId);
        if (!deleted)
            return NotFound(new { success = false, message = $"Chat record #{id} not found." });

        return Ok(new { success = true, message = $"Chat record #{id} has been deleted successfully." });
    }

    // ─── 8. Delete all chat history ──────────────────────────────────────────

    /// <summary>Delete all chat history for the current user.</summary>
    /// <response code="200">All records deleted, returns count of deleted items.</response>
    [HttpDelete("chat/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAllChatHistory()
    {
        var userId  = GetUserId();
        var count   = await _aiService.DeleteAllChatHistoryAsync(userId);

        return Ok(new { success = true, message = $"All chat history deleted successfully.", deletedCount = count });
    }
}
