using Microsoft.AspNetCore.Http;

namespace AiNutritionTracking.API.DTOs.AI;

/// <summary>Request DTO for photo-based body fat analysis (multipart/form-data). Send 1–3 body images.</summary>
public class BodyFatImageRequestDto
{
    public List<IFormFile> Images { get; set; } = new();
    public string Gender { get; set; } = string.Empty;
    public int Age { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
}

/// <summary>Request DTO for measurement-based body fat analysis (application/json).</summary>
public class BodyFatMeasurementRequestDto
{
    public string Gender { get; set; } = string.Empty;
    public int Age { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public double Waist { get; set; }
    public double Neck { get; set; }

    /// <summary>Required for female US Navy formula.</summary>
    public double? Hip { get; set; }
}
