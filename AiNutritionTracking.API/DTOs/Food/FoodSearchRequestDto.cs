namespace AiNutritionTracking.API.DTOs.Food
{
    public class FoodSearchRequestDto
    {
        public string? Query { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}