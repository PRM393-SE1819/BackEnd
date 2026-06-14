using AiNutritionTracking.API.DTOs.Admin.CommunityModeration;
using AiNutritionTracking.API.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiNutritionTracking.API.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]

    public class CommunityModerationController : ControllerBase
    {
        private readonly ICommunityModerationService _service;

        public CommunityModerationController(
            ICommunityModerationService service)
        {
            _service = service;
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            var result = await _service.GetReportsAsync();
            return Ok(result);
        }

        [HttpGet("reports/{id}")]
        public async Task<IActionResult> GetReportDetail(int id)
        {
            var result = await _service.GetReportDetailAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("reports/{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            UpdateReportStatusDto dto)
        {
            var success = await _service.UpdateReportStatusAsync(
                id,
                dto);

            if (!success)
                return NotFound();

            return Ok(new
            {
                Message = "Report status updated successfully"
            });
        }

        [HttpGet("reported-posts")]
        public async Task<IActionResult> GetReportedPosts()
        {
            var result = await _service.GetReportedPostsAsync();
            return Ok(result);
        }

        [HttpGet("reported-comments")]
        public async Task<IActionResult> GetReportedComments()
        {
            var result = await _service.GetReportedCommentsAsync();
            return Ok(result);
        }
    }
}