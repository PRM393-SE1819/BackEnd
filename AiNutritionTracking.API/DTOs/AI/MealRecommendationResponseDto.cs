namespace AiNutritionTracking.API.DTOs.AI;

public class MealRecommendationResponseDto
{
    public MealItemDto Breakfast { get; set; } = new();
    public MealItemDto Lunch { get; set; } = new();
    public MealItemDto Dinner { get; set; } = new();
    public MealItemDto Snack { get; set; } = new();
    public int TotalCalories { get; set; }
    public string Notes { get; set; } = string.Empty;
}
