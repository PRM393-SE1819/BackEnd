using AiNutritionTracking.API.DTOs.Admin.Dashboard;

namespace AiNutritionTracking.API.Services.Admin.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();

        Task<UserStatisticsDto> GetUserStatisticsAsync();

        Task<FoodStatisticsDto> GetFoodStatisticsAsync();

        Task<CommunityStatisticsDto> GetCommunityStatisticsAsync();

        Task<List<UserGrowthDto>> GetUserGrowthAsync();

        Task<List<ReportTrendDto>> GetReportTrendsAsync();

        Task<DashboardDto> GetDashboardAsync();
    }
}