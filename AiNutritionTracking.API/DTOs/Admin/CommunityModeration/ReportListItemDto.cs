namespace AiNutritionTracking.API.DTOs.Admin.CommunityModeration
{
    public class ReportListItemDto
    {
        public int ReportId { get; set; }

        public string TargetType { get; set; } = null!;

        public int TargetId { get; set; }

        public string Reason { get; set; } = null!;

        public string Status { get; set; } = null!;

        public int ReporterUserId { get; set; }

        public string ReporterName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
