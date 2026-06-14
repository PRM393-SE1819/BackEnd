namespace AiNutritionTracking.API.DTOs.Admin.CommunityModeration
{
    public class ReportTargetDto
    {
        public int TargetId { get; set; }

        public string TargetType { get; set; } = null!;

        public string Content { get; set; } = null!;

        public int OwnerUserId { get; set; }

        public string OwnerName { get; set; } = null!;
    }
}
