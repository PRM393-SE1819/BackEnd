using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.DTOs.Meal
{
    public class MealDto
    {
        public int MealId { get; set; }
        public string MealType { get; set; } = string.Empty;
        public DateTime MealDate { get; set; }
        public string? Notes { get; set; }
        public float TotalCalories { get; set; }
        public List<MealItemDto> Items { get; set; } = new();
    }
}