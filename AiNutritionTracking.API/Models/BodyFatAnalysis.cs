namespace AiNutritionTracking.API.Models;

public class BodyFatAnalysis
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public double EstimatedBodyFat { get; set; }
    public string Category { get; set; } = string.Empty;
    public string HealthAssessment { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public double? TargetWeight { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
