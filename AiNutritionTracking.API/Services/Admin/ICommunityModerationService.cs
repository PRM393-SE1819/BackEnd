using AiNutritionTracking.API.DTOs.Admin.CommunityModeration;

namespace AiNutritionTracking.API.Services.Admin
{
    public interface ICommunityModerationService
    {
        Task<List<ReportListItemDto>> GetReportsAsync();

        Task<ReportDetailDto?> GetReportDetailAsync(int reportId);

        Task<bool> UpdateReportStatusAsync(
            int reportId,
            UpdateReportStatusDto dto);

        Task<List<ReportedPostDto>> GetReportedPostsAsync();

        Task<List<ReportedCommentDto>> GetReportedCommentsAsync();
    }
}