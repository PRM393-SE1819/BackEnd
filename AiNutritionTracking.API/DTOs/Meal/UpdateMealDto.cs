using System.Collections.Generic;

namespace AiNutritionTracking.API.DTOs.Meal
{
    public class UpdateMealDto
    {
        public string MealType { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<UpdateMealItemDto> Items { get; set; } = new();
    }
}