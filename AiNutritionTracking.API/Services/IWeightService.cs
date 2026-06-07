using AiNutritionTracking.API.DTOs.Weight;

namespace AiNutritionTracking.API.Services
{
    public interface IWeightService
    {
        Task<WeightLogResponseDto> CreateWeightLogAsync(
            int userId,
            CreateWeightLogDto dto);

        Task<WeightLogResponseDto?> UpdateWeightLogAsync(
            int userId,
            int weightLogId,
            UpdateWeightLogDto dto);

        Task<bool> DeleteWeightLogAsync(
            int userId,
            int weightLogId);

        Task<List<WeightLogResponseDto>> GetWeightLogsAsync(
            int userId,
            int page,
            int pageSize);

        Task<WeightSummaryDto> GetWeightSummaryAsync(
            int userId);

        Task<ProgressStatisticsDto> GetProgressStatisticsAsync(
            int userId,
            DateTime? startDate,
            DateTime? endDate);
    }
}