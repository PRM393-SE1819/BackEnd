using System;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Nutrition;

namespace AiNutritionTracking.API.Services
{
    public interface INutritionService
    {
        Task<NutritionProgressDto> GetCaloriesTrackingAsync(int userId, DateTime date);
        Task<NutritionProgressDto> GetProteinTrackingAsync(int userId, DateTime date);
        Task<NutritionProgressDto> GetCarbTrackingAsync(int userId, DateTime date);
        Task<NutritionProgressDto> GetFatTrackingAsync(int userId, DateTime date);
        Task<NutritionSummaryDto> GetDailySummaryAsync(int userId, DateTime date);
        Task<WeeklyStatisticsDto> GetWeeklyStatisticsAsync(int userId, DateTime startDate);
    }
}