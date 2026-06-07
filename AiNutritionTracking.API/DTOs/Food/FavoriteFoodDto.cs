using System;

namespace AiNutritionTracking.API.DTOs.Food
{
    public class FavoriteFoodDto
    {
        public int FavoriteFoodId { get; set; }
        public int FoodId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public FoodDetailDto? Food { get; set; }
    }
}