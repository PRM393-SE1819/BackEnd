namespace AiNutritionTracking.API.DTOs.AI;

public class MealRecommendationRequestDto
{
    public string Goal { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Height { get; set; }
    public string ActivityLevel { get; set; } = string.Empty;
}
