using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Nutrition;

namespace AiNutritionTracking.API.Services
{
    public class NutritionService : INutritionService
    {
        private readonly AinutritiontrackingContext _context;

        public NutritionService(AinutritiontrackingContext context)
        {
            _context = context;
        }

        private float CalculatePercentage(float consumed, float target)
        {
            if (target <= 0) return 0;
            return (float)Math.Round((consumed / target) * 100, 2);
        }

        private async Task<NutritionSummaryDto> GetSummaryInternalAsync(int userId, DateTime date)
        {
            var dateOnly = DateOnly.FromDateTime(date);
            var user = await _context.Users.FindAsync(userId);

            float calTarget = user?.CaloriesTarget ?? 2000;
            float proTarget = (float)(user?.ProteinTarget ?? 50);
            float carbTarget = (float)(user?.CarbTarget ?? 250);
            float fatTarget = (float)(user?.FatTarget ?? 70);

            var meals = await _context.Meals
                .Include(m => m.MealItems)
                .Where(m => m.UserId == userId && m.MealDate == dateOnly)
                .ToListAsync();

            var allItems = meals.SelectMany(m => m.MealItems).ToList();

            float caloriesConsumed = (float)allItems.Sum(i => i.Calories ?? 0);
            float proteinConsumed = (float)allItems.Sum(i => i.Protein ?? 0);
            float carbConsumed = (float)allItems.Sum(i => i.Carbs ?? 0);
            float fatConsumed = (float)allItems.Sum(i => i.Fat ?? 0);

            return new NutritionSummaryDto
            {
                Date = date,
                CaloriesConsumed = caloriesConsumed,
                CaloriesTarget = calTarget,
                ProteinConsumed = proteinConsumed,
                ProteinTarget = proTarget,
                CarbConsumed = carbConsumed,
                CarbTarget = carbTarget,
                FatConsumed = fatConsumed,
                FatTarget = fatTarget
            };
        }

        public async Task<NutritionProgressDto> GetCaloriesTrackingAsync(int userId, DateTime date)
        {
            var summary = await GetSummaryInternalAsync(userId, date);
            return new NutritionProgressDto
            {
                Nutrient = "Calories",
                Consumed = summary.CaloriesConsumed,
                Target = summary.CaloriesTarget,
                Remaining = Math.Max(0, summary.CaloriesTarget - summary.CaloriesConsumed),
                Percentage = CalculatePercentage(summary.CaloriesConsumed, summary.CaloriesTarget)
            };
        }

        public async Task<NutritionProgressDto> GetProteinTrackingAsync(int userId, DateTime date)
        {
            var summary = await GetSummaryInternalAsync(userId, date);
            return new NutritionProgressDto
            {
                Nutrient = "Protein",
                Consumed = summary.ProteinConsumed,
                Target = summary.ProteinTarget,
                Remaining = Math.Max(0, summary.ProteinTarget - summary.ProteinConsumed),
                Percentage = CalculatePercentage(summary.ProteinConsumed, summary.ProteinTarget)
            };
        }

        public async Task<NutritionProgressDto> GetCarbTrackingAsync(int userId, DateTime date)
        {
            var summary = await GetSummaryInternalAsync(userId, date);
            return new NutritionProgressDto
            {
                Nutrient = "Carbs",
                Consumed = summary.CarbConsumed,
                Target = summary.CarbTarget,
                Remaining = Math.Max(0, summary.CarbTarget - summary.CarbConsumed),
                Percentage = CalculatePercentage(summary.CarbConsumed, summary.CarbTarget)
            };
        }

        public async Task<NutritionProgressDto> GetFatTrackingAsync(int userId, DateTime date)
        {
            var summary = await GetSummaryInternalAsync(userId, date);
            return new NutritionProgressDto
            {
                Nutrient = "Fat",
                Consumed = summary.FatConsumed,
                Target = summary.FatTarget,
                Remaining = Math.Max(0, summary.FatTarget - summary.FatConsumed),
                Percentage = CalculatePercentage(summary.FatConsumed, summary.FatTarget)
            };
        }

        public async Task<NutritionSummaryDto> GetDailySummaryAsync(int userId, DateTime date)
        {
            return await GetSummaryInternalAsync(userId, date);
        }

        public async Task<WeeklyStatisticsDto> GetWeeklyStatisticsAsync(int userId, DateTime startDate)
        {
            var endDate = startDate.AddDays(6);
            var startOnly = DateOnly.FromDateTime(startDate);
            var endOnly = DateOnly.FromDateTime(endDate);

            var meals = await _context.Meals
                .Include(m => m.MealItems)
                .Where(m => m.UserId == userId && m.MealDate >= startOnly && m.MealDate <= endOnly)
                .ToListAsync();

            var stats = new WeeklyStatisticsDto();

            for (int i = 0; i < 7; i++)
            {
                var currentDay = startDate.AddDays(i);
                var currentDayOnly = DateOnly.FromDateTime(currentDay);

                var dayMeals = meals.Where(m => m.MealDate == currentDayOnly).SelectMany(m => m.MealItems).ToList();

                stats.Statistics.Add(new WeeklyNutritionDto
                {
                    Date = currentDay,
                    Calories = (float)dayMeals.Sum(mi => mi.Calories ?? 0),
                    Protein = (float)dayMeals.Sum(mi => mi.Protein ?? 0),
                    Carbs = (float)dayMeals.Sum(mi => mi.Carbs ?? 0),
                    Fat = (float)dayMeals.Sum(mi => mi.Fat ?? 0)
                });
            }

            if (stats.Statistics.Any())
            {
                stats.AverageCalories = stats.Statistics.Average(s => s.Calories);
                stats.AverageProtein = stats.Statistics.Average(s => s.Protein);
                stats.AverageCarbs = stats.Statistics.Average(s => s.Carbs);
                stats.AverageFat = stats.Statistics.Average(s => s.Fat);
            }

            return stats;
        }
    }
}