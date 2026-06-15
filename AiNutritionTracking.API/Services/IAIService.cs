using AiNutritionTracking.API.DTOs.AI;

namespace AiNutritionTracking.API.Services;

public interface IAIService
{
    Task<ChatResponseDto> ChatAsync(string message, int userId);
    Task<FoodImageAnalysisResponseDto> AnalyzeImageAsync(IFormFile image, int userId);
    Task<CalorieEstimateResponseDto> EstimateCaloriesAsync(string foodDescription, int userId);
    Task<MealRecommendationResponseDto> GetMealRecommendationsAsync(MealRecommendationRequestDto request, int userId);
    Task<MealPlanResponseDto> GetMealPlanAsync(MealPlanRequestDto request, int userId);
    Task<List<ChatHistoryDto>> GetChatHistoryAsync(int userId, int page, int pageSize);
}
