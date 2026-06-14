namespace AiNutritionTracking.API.DTOs.Admin.CommunityModeration
{
    public class ReportReporterDto
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string AvatarUrl { get; set; } = null!;
    }
}
