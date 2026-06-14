namespace AiNutritionTracking.API.DTOs.AI;

public class MealItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
