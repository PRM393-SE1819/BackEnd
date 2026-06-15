namespace AiNutritionTracking.API.DTOs.Admin.CommunityModeration
{
    public class ReportDetailDto
    {
        public int ReportId { get; set; }

        public string Reason { get; set; } = null!;

        public string Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public ReportReporterDto Reporter { get; set; } = null!;

        public ReportTargetDto Target { get; set; } = null!;
    }
}
