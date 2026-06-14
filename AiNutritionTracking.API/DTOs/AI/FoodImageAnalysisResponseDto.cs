namespace AiNutritionTracking.API.DTOs.AI;

public class FoodImageAnalysisResponseDto
{
    public string FoodName { get; set; } = string.Empty;
    public double EstimatedCalories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}
