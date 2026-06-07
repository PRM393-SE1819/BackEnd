using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Weight;
using AiNutritionTracking.API.Models;
using Google;
using Microsoft.EntityFrameworkCore;

namespace AiNutritionTracking.API.Services
{
    public class WeightService : IWeightService
    {
        private readonly AinutritiontrackingContext _context;

        public WeightService(AinutritiontrackingContext context)
        {
            _context = context;
        }

        public async Task<WeightLogResponseDto> CreateWeightLogAsync(
          int userId,
          CreateWeightLogDto dto)
        {
            var weightLog = new WeightLog
            {
                UserId = userId,
                Weight = dto.Weight,
                BodyFat = dto.BodyFat,
                LoggedAt = DateTime.UtcNow
            };

            await _context.WeightLogs.AddAsync(weightLog);
            await _context.SaveChangesAsync();

            return new WeightLogResponseDto
            {
                WeightLogId = weightLog.WeightLogId,
                Weight = weightLog.Weight,
                BodyFat = weightLog.BodyFat,
                LoggedAt = weightLog.LoggedAt
            };
        }

        public async Task<WeightLogResponseDto?> UpdateWeightLogAsync(
       int userId,
       int weightLogId,
       UpdateWeightLogDto dto)
        {
            var weightLog = await _context.WeightLogs
                .FirstOrDefaultAsync(x =>
                    x.WeightLogId == weightLogId &&
                    x.UserId == userId);

            if (weightLog == null)
                return null;

            weightLog.Weight = dto.Weight;
            weightLog.BodyFat = dto.BodyFat;

            await _context.SaveChangesAsync();

            return new WeightLogResponseDto
            {
                WeightLogId = weightLog.WeightLogId,
                Weight = weightLog.Weight,
                BodyFat = weightLog.BodyFat,
                LoggedAt = weightLog.LoggedAt
            };
        }

        public async Task<bool> DeleteWeightLogAsync(
         int userId,
         int weightLogId)
        {
            var weightLog = await _context.WeightLogs
                .FirstOrDefaultAsync(x =>
                    x.WeightLogId == weightLogId &&
                    x.UserId == userId);

            if (weightLog == null)
                return false;

            _context.WeightLogs.Remove(weightLog);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<WeightLogResponseDto>> GetWeightLogsAsync(
            int userId,
            int page,
            int pageSize)
        {
            return await _context.WeightLogs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.LoggedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WeightLogResponseDto
                {
                    WeightLogId = x.WeightLogId,
                    Weight = x.Weight,
                    BodyFat = x.BodyFat,
                    LoggedAt = x.LoggedAt
                })
                .ToListAsync();
        }

        public async Task<WeightSummaryDto> GetWeightSummaryAsync(
            int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                throw new Exception("User not found");

            var latestLog = await _context.WeightLogs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.LoggedAt)
                .FirstOrDefaultAsync();

            return new WeightSummaryDto
            {
                CurrentWeight =
                    latestLog?.Weight ?? user.Weight ?? 0,

                TargetWeight =
                    user.TargetWeight ?? 0,

                WeightDifference =
                    (latestLog?.Weight ?? user.Weight ?? 0)
                    - (user.TargetWeight ?? 0),

                CurrentBodyFat =
                    latestLog?.BodyFat ?? user.BodyFat
            };
        }

        public async Task<ProgressStatisticsDto>
            GetProgressStatisticsAsync(
            int userId,
            DateTime? startDate,
            DateTime? endDate)
        {
            startDate ??= DateTime.UtcNow.AddMonths(-1);
            endDate ??= DateTime.UtcNow;

            var logs = await _context.WeightLogs
                .Where(x =>
                    x.UserId == userId &&
                    x.LoggedAt >= startDate &&
                    x.LoggedAt <= endDate)
                .OrderBy(x => x.LoggedAt)
                .ToListAsync();

            if (!logs.Any())
            {
                return new ProgressStatisticsDto
                {
                    History = new List<WeightChartDto>()
                };
            }

            var first = logs.First();
            var last = logs.Last();

            return new ProgressStatisticsDto
            {
                StartWeight = first.Weight,
                CurrentWeight = last.Weight,
                WeightChanged = last.Weight - first.Weight,

                StartBodyFat = first.BodyFat,
                CurrentBodyFat = last.BodyFat,

                BodyFatChanged =
                    first.BodyFat.HasValue &&
                    last.BodyFat.HasValue
                    ? last.BodyFat.Value - first.BodyFat.Value
                    : null,

                History = logs.Select(x => new WeightChartDto
                {
                    Date = x.LoggedAt,
                    Weight = x.Weight,
                    BodyFat = x.BodyFat
                }).ToList()
            };
        }
    }
}