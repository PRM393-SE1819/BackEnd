namespace AiNutritionTracking.API.DTOs.Admin.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int BannedUsers { get; set; }

        public int TotalFoods { get; set; }

        public int VerifiedFoods { get; set; }

        public int TotalPosts { get; set; }

        public int PendingReports { get; set; }

        public int TotalAIRequests { get; set; }
    }
}
