namespace AiNutritionTracking.API.Services;

public interface IPexelsService
{
    Task<string> SearchFoodImageAsync(string foodName);
}
