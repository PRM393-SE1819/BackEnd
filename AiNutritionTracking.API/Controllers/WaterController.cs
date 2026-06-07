using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Water;
using AiNutritionTracking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiNutritionTracking.API.Controllers
{
    [ApiController]
    [Route("api/water")]
    [Authorize]
    public class WaterController : ControllerBase
    {
        private readonly IWaterService _waterService;

        public WaterController(IWaterService waterService)
        {
            _waterService = waterService;
        }

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không tìm th?y thông tin ??nh danh ng??i dùng trong Token.");
        }

        [HttpPost("logs")]
        public async Task<IActionResult> AddWaterLog([FromBody] CreateWaterLogDto request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _waterService.AddWaterLogAsync(userId, request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetWaterLogHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] DateTime? date = null)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _waterService.GetWaterLogHistoryAsync(userId, page, pageSize, date);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpDelete("logs/{waterLogId}")]
        public async Task<IActionResult> DeleteWaterLog(int waterLogId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var success = await _waterService.DeleteWaterLogAsync(userId, waterLogId);
                if (!success) return NotFound(new { message = "Water log not found or unauthorized." });
                return Ok(new { message = "Water log deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("goal")]
        public async Task<IActionResult> GetWaterGoal()
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _waterService.GetWaterGoalAsync(userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("goal")]
        public async Task<IActionResult> UpdateWaterGoal([FromBody] UpdateWaterGoalDto request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _waterService.UpdateWaterGoalAsync(userId, request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("daily-summary")]
        public async Task<IActionResult> GetDailyWaterSummary([FromQuery] DateTime date)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _waterService.GetDailyWaterSummaryAsync(userId, date);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("reminders")]
        public async Task<IActionResult> CreateReminder([FromBody] CreateWaterReminderDto request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _waterService.CreateReminderAsync(userId, request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("reminders")]
        public async Task<IActionResult> GetReminders()
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _waterService.GetRemindersAsync(userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("reminders/{reminderId}")]
        public async Task<IActionResult> UpdateReminder(int reminderId, [FromBody] CreateWaterReminderDto request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _waterService.UpdateReminderAsync(userId, reminderId, request);
                if (result == null) return NotFound(new { message = "Reminder not found or unauthorized." });
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpDelete("reminders/{reminderId}")]
        public async Task<IActionResult> DeleteReminder(int reminderId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var success = await _waterService.DeleteReminderAsync(userId, reminderId);
                if (!success) return NotFound(new { message = "Reminder not found or unauthorized." });
                return Ok(new { message = "Reminder deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
