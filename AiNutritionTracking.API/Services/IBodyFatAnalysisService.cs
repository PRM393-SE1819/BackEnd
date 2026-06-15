using AiNutritionTracking.API.DTOs.AI;

namespace AiNutritionTracking.API.Services;

public interface IBodyFatAnalysisService
{
    Task<BodyFatAnalysisResponseDto> AnalyzeFromImagesAsync(BodyFatImageRequestDto request, int userId);
    Task<BodyFatAnalysisResponseDto> AnalyzeFromMeasurementsAsync(BodyFatMeasurementRequestDto request, int userId);
}
