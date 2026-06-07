using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AiNutritionTracking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiNutritionTracking.API.Controllers
{
    [ApiController]
    [Route("api/nutrition")]
    [Authorize]
    public class NutritionController : ControllerBase
    {
        private readonly INutritionService _nutritionService;

        public NutritionController(INutritionService nutritionService)
        {
            _nutritionService = nutritionService;
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

        [HttpGet("calories")]
        public async Task<IActionResult> GetCaloriesTracking([FromQuery] DateTime date)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _nutritionService.GetCaloriesTrackingAsync(userId, date);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("protein")]
        public async Task<IActionResult> GetProteinTracking([FromQuery] DateTime date)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _nutritionService.GetProteinTrackingAsync(userId, date);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("carbs")]
        public async Task<IActionResult> GetCarbTracking([FromQuery] DateTime date)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _nutritionService.GetCarbTrackingAsync(userId, date);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("fat")]
        public async Task<IActionResult> GetFatTracking([FromQuery] DateTime date)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _nutritionService.GetFatTrackingAsync(userId, date);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("daily-summary")]
        public async Task<IActionResult> GetDailyNutritionSummary([FromQuery] DateTime date)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _nutritionService.GetDailySummaryAsync(userId, date);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("weekly-statistics")]
        public async Task<IActionResult> GetWeeklyStatistics([FromQuery] DateTime startDate)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _nutritionService.GetWeeklyStatisticsAsync(userId, startDate);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}