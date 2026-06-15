namespace AiNutritionTracking.API.DTOs.AI;

public class MealPlanRequestDto
{
    public string Goal { get; set; } = string.Empty;
    public int DailyCalories { get; set; }
}
