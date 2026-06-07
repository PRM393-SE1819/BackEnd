using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Water;

namespace AiNutritionTracking.API.Services
{
    public interface IWaterService
    {
        Task<WaterLogDto> AddWaterLogAsync(int userId, CreateWaterLogDto request);
        Task<List<WaterLogDto>> GetWaterLogHistoryAsync(int userId, int page, int pageSize, DateTime? date);
        Task<bool> DeleteWaterLogAsync(int userId, int waterLogId);

        Task<WaterGoalDto> GetWaterGoalAsync(int userId);
        Task<WaterGoalDto> UpdateWaterGoalAsync(int userId, UpdateWaterGoalDto request);

        Task<WaterSummaryDto> GetDailyWaterSummaryAsync(int userId, DateTime date);

        Task<WaterReminderDto> CreateReminderAsync(int userId, CreateWaterReminderDto request);
        Task<List<WaterReminderDto>> GetRemindersAsync(int userId);
        Task<WaterReminderDto?> UpdateReminderAsync(int userId, int reminderId, CreateWaterReminderDto request);
        Task<bool> DeleteReminderAsync(int userId, int reminderId);
    }
}
