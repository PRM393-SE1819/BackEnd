namespace AiNutritionTracking.API.DTOs.Admin.Dashboard
{
    public class CommunityStatisticsDto
    {
        public int TotalPosts { get; set; }

        public int TotalComments { get; set; }

        public int PendingReports { get; set; }

        public int ApprovedReports { get; set; }

        public int RejectedReports { get; set; }
    }
}
