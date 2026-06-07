using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Meal;
using AiNutritionTracking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiNutritionTracking.API.Controllers
{
    [ApiController]
    [Route("api/meals")]
    [Authorize]
    public class MealController : ControllerBase
    {
        private readonly IMealService _mealService;

        public MealController(IMealService mealService)
        {
            _mealService = mealService;
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

        [HttpPost]
        public async Task<IActionResult> AddMeal([FromBody] CreateMealDto request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _mealService.AddMealAsync(userId, request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("{mealId}")]
        public async Task<IActionResult> UpdateMeal(int mealId, [FromBody] UpdateMealDto request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _mealService.UpdateMealAsync(userId, mealId, request);
                if (result == null) return NotFound(new { message = "Meal not found or unauthorized." });
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpDelete("{mealId}")]
        public async Task<IActionResult> DeleteMeal(int mealId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var success = await _mealService.DeleteMealAsync(userId, mealId);
                if (!success) return NotFound(new { message = "Meal not found or unauthorized." });
                return Ok(new { message = "Meal deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("{mealId}")]
        public async Task<IActionResult> GetMealDetail(int mealId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _mealService.GetMealDetailAsync(userId, mealId);
                if (result == null) return NotFound(new { message = "Meal not found or unauthorized." });
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetMealHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? date = null,
            [FromQuery] string? mealType = null)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _mealService.GetMealHistoryAsync(userId, page, pageSize, date, mealType);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("daily-summary")]
        public async Task<IActionResult> GetDailyCaloriesSummary([FromQuery] DateTime date)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _mealService.GetDailyCaloriesSummaryAsync(userId, date);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}