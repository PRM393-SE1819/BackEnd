namespace AiNutritionTracking.API.DTOs.AI;

public class MealPlanResponseDto
{
    public List<MealPlanDayDto> Days { get; set; } = new();
}

public class MealPlanDayDto
{
    public string Day { get; set; } = string.Empty;
    public MealItemDto Breakfast { get; set; } = new();
    public MealItemDto Lunch { get; set; } = new();
    public MealItemDto Dinner { get; set; } = new();
    public MealItemDto Snack { get; set; } = new();
    public int TotalCalories { get; set; }
}
