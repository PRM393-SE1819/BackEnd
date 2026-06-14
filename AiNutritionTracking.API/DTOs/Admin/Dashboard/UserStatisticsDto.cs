namespace AiNutritionTracking.API.DTOs.Admin.Dashboard
{
    public class UserStatisticsDto
    {
        public int NewUsersToday { get; set; }

        public int NewUsersThisWeek { get; set; }

        public int NewUsersThisMonth { get; set; }

        public int TotalUsers { get; set; }
    }
}
