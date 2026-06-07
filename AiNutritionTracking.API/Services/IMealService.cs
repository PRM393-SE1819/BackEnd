using System;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Meal;

namespace AiNutritionTracking.API.Services
{
    public interface IMealService
    {
        Task<MealDto> AddMealAsync(int userId, CreateMealDto request);
        Task<MealDto?> UpdateMealAsync(int userId, int mealId, UpdateMealDto request);
        Task<bool> DeleteMealAsync(int userId, int mealId);
        Task<MealDto?> GetMealDetailAsync(int userId, int mealId);
        Task<PagedResponse<MealDto>> GetMealHistoryAsync(int userId, int page, int pageSize, DateTime? date, string? mealType);
        Task<DailyCaloriesSummaryDto> GetDailyCaloriesSummaryAsync(int userId, DateTime date);
    }
}