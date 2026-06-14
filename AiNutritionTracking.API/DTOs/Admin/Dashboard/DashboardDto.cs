namespace AiNutritionTracking.API.DTOs.Admin.Dashboard
{
    public class DashboardDto
    {
        public DashboardSummaryDto Summary { get; set; } = null!;

        public UserStatisticsDto UserStatistics { get; set; } = null!;

        public FoodStatisticsDto FoodStatistics { get; set; } = null!;

        public CommunityStatisticsDto CommunityStatistics { get; set; } = null!;

        public List<UserGrowthDto> UserGrowth { get; set; } = new();

        public List<ReportTrendDto> ReportTrends { get; set; } = new();
    }
}
