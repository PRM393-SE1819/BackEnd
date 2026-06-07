using System.Collections.Generic;

namespace AiNutritionTracking.API.DTOs.Food
{
    public class FoodSearchResponseDto
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public List<FoodDetailDto> Items { get; set; } = new List<FoodDetailDto>();
    }
}