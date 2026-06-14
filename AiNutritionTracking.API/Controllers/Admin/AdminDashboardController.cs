using AiNutritionTracking.API.DTOs.Admin.Dashboard;
using AiNutritionTracking.API.Services.Admin;
using AiNutritionTracking.API.Services.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiNutritionTracking.API.Controllers.Admin
{
    [Route("api/admin/dashboard")]
    [ApiController]
   
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminDashboardController(
            IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _dashboardService.GetSummaryAsync();

            return Ok(result);
        }

        [HttpGet("users")]
        [ProducesResponseType(typeof(UserStatisticsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserStatistics()
        {
            var result = await _dashboardService.GetUserStatisticsAsync();

            return Ok(result);
        }

  
        [HttpGet("foods")]
        [ProducesResponseType(typeof(FoodStatisticsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFoodStatistics()
        {
            var result = await _dashboardService.GetFoodStatisticsAsync();

            return Ok(result);
        }

    
        [HttpGet("community")]
        [ProducesResponseType(typeof(CommunityStatisticsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCommunityStatistics()
        {
            var result = await _dashboardService.GetCommunityStatisticsAsync();

            return Ok(result);
        }

     
        [HttpGet("user-growth")]
        [ProducesResponseType(typeof(List<UserGrowthDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserGrowth()
        {
            var result = await _dashboardService.GetUserGrowthAsync();

            return Ok(result);
        }

     
        [HttpGet("report-trends")]
        [ProducesResponseType(typeof(List<ReportTrendDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReportTrends()
        {
            var result = await _dashboardService.GetReportTrendsAsync();

            return Ok(result);
        }

   
        [HttpGet]
        [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _dashboardService.GetDashboardAsync();

            return Ok(result);
        }
    }
}