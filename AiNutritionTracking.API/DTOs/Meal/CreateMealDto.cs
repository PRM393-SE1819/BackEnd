using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.DTOs.Meal
{
    public class CreateMealDto
    {
        public string MealType { get; set; } = string.Empty;
        public DateTime MealDate { get; set; }
        public string? Notes { get; set; }
        public List<CreateMealItemDto> Items { get; set; } = new();
    }
}