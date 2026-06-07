using System.Security.Claims;
using AiNutritionTracking.API.DTOs.Weight;
using AiNutritionTracking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiNutritionTracking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WeightController : ControllerBase
    {
        private readonly IWeightService _weightService;

        public WeightController(IWeightService weightService)
        {
            _weightService = weightService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found.");
            }

            return int.Parse(userIdClaim);
        }
        [HttpPost("logs")]
        public async Task<IActionResult> CreateWeightLog(
     [FromBody] CreateWeightLogDto dto)
        {
            var userId = GetCurrentUserId();

            var result = await _weightService.CreateWeightLogAsync(userId, dto);

            return CreatedAtAction(
                nameof(GetWeightLogs),
                new { },
                result);
        }


        [HttpPut("logs/{weightLogId}")]
        public async Task<IActionResult> UpdateWeightLog(
            int weightLogId,
            [FromBody] UpdateWeightLogDto dto)
        {
            var userId = GetCurrentUserId();

            var result = await _weightService
                .UpdateWeightLogAsync(userId, weightLogId, dto);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Weight log not found"
                });
            }

            return Ok(result);
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetWeightLogs(
            [FromQuery] WeightLogQueryDto query)
        {
            var userId = GetCurrentUserId();

            var result = await _weightService
                .GetWeightLogsAsync(
                    userId,
                    query.Page,
                    query.PageSize);

            return Ok(result);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetWeightSummary()
        {
            var userId = GetCurrentUserId();

            var result = await _weightService
                .GetWeightSummaryAsync(userId);

            return Ok(result);
        }

        [HttpGet("progress-statistics")]
        public async Task<IActionResult> GetProgressStatistics(
            [FromQuery] ProgressStatisticsQueryDto query)
        {
            var userId = GetCurrentUserId();

            var result = await _weightService
                .GetProgressStatisticsAsync(
                    userId,
                    query.StartDate,
                    query.EndDate);

            return Ok(result);
        }
    }
}