using System;

namespace AiNutritionTracking.API.DTOs.Meal
{
    public class DailyCaloriesSummaryDto
    {
        public DateTime Date { get; set; }
        public float CaloriesConsumed { get; set; }
        public float Protein { get; set; }
        public float Carbs { get; set; }
        public float Fat { get; set; }
        public int CaloriesTarget { get; set; }
        public float RemainingCalories { get; set; }
    }
}