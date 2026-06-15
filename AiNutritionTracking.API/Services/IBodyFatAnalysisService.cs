using AiNutritionTracking.API.DTOs.AI;

namespace AiNutritionTracking.API.Services;

public interface IBodyFatAnalysisService
{
    Task<BodyFatAnalysisResponseDto> AnalyzeFromImagesAsync(BodyFatImageRequestDto request, int userId);
    Task<BodyFatAnalysisResponseDto> AnalyzeFromMeasurementsAsync(BodyFatMeasurementRequestDto request, int userId);
    Task<List<BodyFatHistoryDto>> GetHistoryAsync(int userId);
    Task<BodyFatHistoryDto?> GetHistoryByIdAsync(int id, int userId);
    Task<bool> DeleteHistoryAsync(int id, int userId);
}
