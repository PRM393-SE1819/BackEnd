using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Water;
using AiNutritionTracking.API.Models;

namespace AiNutritionTracking.API.Services
{
    public class WaterService : IWaterService
    {
        private readonly AinutritiontrackingContext _context;

        public WaterService(AinutritiontrackingContext context)
        {
            _context = context;
        }

        public async Task<WaterLogDto> AddWaterLogAsync(int userId, CreateWaterLogDto request)
        {
            var log = new WaterLog
            {
                UserId = userId,
                AmountMl = request.AmountML,
                LoggedAt = DateTime.UtcNow
            };

            _context.WaterLogs.Add(log);
            await _context.SaveChangesAsync();

            return new WaterLogDto
            {
                WaterLogId = log.WaterLogId,
                AmountML = (float)(log.AmountMl ?? 0),
                LoggedAt = log.LoggedAt ?? DateTime.UtcNow
            };
        }

        public async Task<List<WaterLogDto>> GetWaterLogHistoryAsync(int userId, int page, int pageSize, DateTime? date)
        {
            var query = _context.WaterLogs.Where(w => w.UserId == userId).AsQueryable();

            if (date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
                query = query.Where(w => w.LoggedAt >= startOfDay && w.LoggedAt <= endOfDay);
            }

            var logs = await query
                .OrderByDescending(w => w.LoggedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return logs.Select(log => new WaterLogDto
            {
                WaterLogId = log.WaterLogId,
                AmountML = (float)(log.AmountMl ?? 0),
                LoggedAt = log.LoggedAt ?? DateTime.UtcNow
            }).ToList();
        }

        public async Task<bool> DeleteWaterLogAsync(int userId, int waterLogId)
        {
            var log = await _context.WaterLogs.FirstOrDefaultAsync(w => w.WaterLogId == waterLogId && w.UserId == userId);
            if (log == null) return false;

            _context.WaterLogs.Remove(log);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WaterGoalDto> GetWaterGoalAsync(int userId)
        {
            var goal = await _context.WaterGoals.FirstOrDefaultAsync(g => g.UserId == userId);
            return new WaterGoalDto
            {
                DailyTargetML = (float)(goal?.DailyTargetMl ?? 2000)
            };
        }

        public async Task<WaterGoalDto> UpdateWaterGoalAsync(int userId, UpdateWaterGoalDto request)
        {
            var goal = await _context.WaterGoals.FirstOrDefaultAsync(g => g.UserId == userId);
            if (goal == null)
            {
                goal = new WaterGoal
                {
                    UserId = userId,
                    DailyTargetMl = request.DailyTargetML,
                    CreatedAt = DateTime.UtcNow
                };
                _context.WaterGoals.Add(goal);
            }
            else
            {
                goal.DailyTargetMl = request.DailyTargetML;
                goal.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new WaterGoalDto
            {
                DailyTargetML = (float)goal.DailyTargetMl
            };
        }

        public async Task<WaterSummaryDto> GetDailyWaterSummaryAsync(int userId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var logs = await _context.WaterLogs
                .Where(w => w.UserId == userId && w.LoggedAt >= startOfDay && w.LoggedAt <= endOfDay)
                .ToListAsync();

            float consumedML = (float)logs.Sum(l => l.AmountMl ?? 0);

            var goal = await GetWaterGoalAsync(userId);
            float goalML = goal.DailyTargetML;
            float remainingML = Math.Max(0, goalML - consumedML);
            float percentage = goalML > 0 ? (float)Math.Round((consumedML / goalML) * 100, 2) : 0;

            return new WaterSummaryDto
            {
                Date = startOfDay,
                ConsumedML = consumedML,
                GoalML = goalML,
                RemainingML = remainingML,
                Percentage = percentage
            };
        }

        public async Task<WaterReminderDto> CreateReminderAsync(int userId, CreateWaterReminderDto request)
        {
            var reminder = new WaterReminder
            {
                UserId = userId,
                ReminderTime = request.ReminderTime,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.WaterReminders.Add(reminder);
            await _context.SaveChangesAsync();

            return new WaterReminderDto
            {
                ReminderId = reminder.ReminderId,
                ReminderTime = reminder.ReminderTime,
                IsEnabled = reminder.IsEnabled
            };
        }

        public async Task<List<WaterReminderDto>> GetRemindersAsync(int userId)
        {
            var reminders = await _context.WaterReminders
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.ReminderTime)
                .ToListAsync();

            return reminders.Select(r => new WaterReminderDto
            {
                ReminderId = r.ReminderId,
                ReminderTime = r.ReminderTime,
                IsEnabled = r.IsEnabled
            }).ToList();
        }

        public async Task<WaterReminderDto?> UpdateReminderAsync(int userId, int reminderId, CreateWaterReminderDto request)
        {
            var reminder = await _context.WaterReminders.FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.UserId == userId);
            if (reminder == null) return null;

            reminder.ReminderTime = request.ReminderTime;
            await _context.SaveChangesAsync();

            return new WaterReminderDto
            {
                ReminderId = reminder.ReminderId,
                ReminderTime = reminder.ReminderTime,
                IsEnabled = reminder.IsEnabled
            };
        }

        public async Task<bool> DeleteReminderAsync(int userId, int reminderId)
        {
            var reminder = await _context.WaterReminders.FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.UserId == userId);
            if (reminder == null) return false;

            _context.WaterReminders.Remove(reminder);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
