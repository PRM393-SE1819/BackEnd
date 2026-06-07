using System.Collections.Generic;

namespace AiNutritionTracking.API.DTOs.Nutrition
{
    public class WeeklyStatisticsDto
    {
        public List<WeeklyNutritionDto> Statistics { get; set; } = new();
        public float AverageCalories { get; set; }
        public float AverageProtein { get; set; }
        public float AverageCarbs { get; set; }
        public float AverageFat { get; set; }
    }
}