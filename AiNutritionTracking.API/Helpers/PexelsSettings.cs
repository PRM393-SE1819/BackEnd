namespace AiNutritionTracking.API.Helpers;

public class PexelsSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.pexels.com/v1";
    public string DefaultImageUrl { get; set; } = "https://images.pexels.com/photos/1640777/pexels-photo-1640777.jpeg";
}
