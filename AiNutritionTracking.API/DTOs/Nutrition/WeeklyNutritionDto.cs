using System;

namespace AiNutritionTracking.API.DTOs.Nutrition
{
    public class WeeklyNutritionDto
    {
        public DateTime Date { get; set; }
        public float Calories { get; set; }
        public float Protein { get; set; }
        public float Carbs { get; set; }
        public float Fat { get; set; }
    }
}