namespace AiNutritionTracking.API.DTOs.Admin.CommunityModeration
{
    public class ReportedPostDto
    {
        public int PostId { get; set; }

        public string Content { get; set; } = null!;

        public string? ImageUrl { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = null!;

        public int ReportCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
