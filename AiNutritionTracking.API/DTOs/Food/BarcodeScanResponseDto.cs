namespace AiNutritionTracking.API.DTOs.Food
{
    public class BarcodeScanResponseDto
    {
        public bool Found { get; set; }
        public FoodDetailDto? Food { get; set; }
    }
}