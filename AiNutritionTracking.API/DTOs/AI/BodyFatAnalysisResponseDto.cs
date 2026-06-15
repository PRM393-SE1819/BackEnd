namespace AiNutritionTracking.API.DTOs.AI;

public class BodyFatAnalysisResponseDto
{
    public double EstimatedBodyFat { get; set; }
    public string Category { get; set; } = string.Empty;
    public string HealthAssessment { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public double? TargetWeight { get; set; }
}
