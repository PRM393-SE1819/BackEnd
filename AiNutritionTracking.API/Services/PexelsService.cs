using System.Text.Json;
using AiNutritionTracking.API.Helpers;
using Microsoft.Extensions.Options;

namespace AiNutritionTracking.API.Services;

public class PexelsService : IPexelsService
{
    private readonly HttpClient _httpClient;
    private readonly PexelsSettings _settings;
    private readonly ILogger<PexelsService> _logger;

    public PexelsService(HttpClient httpClient, IOptions<PexelsSettings> settings, ILogger<PexelsService> logger)
    {
        _httpClient = httpClient;
        _settings   = settings.Value;
        _logger     = logger;
    }

    public async Task<string> SearchFoodImageAsync(string foodName)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", _settings.ApiKey);

            var encoded  = Uri.EscapeDataString(foodName);
            var url      = $"{_settings.BaseUrl}/search?query={encoded}&per_page=1&orientation=landscape";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Pexels API returned {Code} for query '{Query}'", response.StatusCode, foodName);
                return _settings.DefaultImageUrl;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var photos = doc.RootElement.GetProperty("photos");
            if (photos.GetArrayLength() == 0)
            {
                _logger.LogInformation("No Pexels images found for '{Query}', using default", foodName);
                return _settings.DefaultImageUrl;
            }

            var imageUrl = photos[0]
                .GetProperty("src")
                .GetProperty("large")
                .GetString();

            return imageUrl ?? _settings.DefaultImageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pexels search failed for '{FoodName}'", foodName);
            return _settings.DefaultImageUrl;
        }
    }
}
