namespace AiNutritionTracking.API.Helpers;

public class GroqSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "llama-3.3-70b-versatile";
    public string VisionModel { get; set; } = "meta-llama/llama-4-scout-17b-16e-instruct";
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";
}
