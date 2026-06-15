using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Admin.CommunityModeration;
using AiNutritionTracking.API.Services.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiNutritionTracking.API.Services.Admin
{
    public class CommunityModerationService : ICommunityModerationService
    {
        private readonly AinutritiontrackingContext _context;

        public CommunityModerationService(
            AinutritiontrackingContext context)
        {
            _context = context;
        }

        public async Task<List<ReportListItemDto>> GetReportsAsync()
        {
            return await _context.Reports
                .Include(r => r.ReporterUser)
                .OrderByDescending(r => r.CreatedAt)
             .Select(r => new ReportListItemDto
             {
                 ReportId = r.ReportId,
                 TargetType = r.TargetType ?? "",
                 TargetId = r.TargetId ?? 0,
                 Reason = r.Reason ?? "",
                 Status = r.Status ?? "",
                 ReporterUserId = r.ReporterUserId ?? 0,
                 ReporterName = r.ReporterUser != null
        ? r.ReporterUser.FullName
        : "",
                 CreatedAt = r.CreatedAt ?? DateTime.Now
             })
                .ToListAsync();
        }

        public async Task<ReportDetailDto?> GetReportDetailAsync(int reportId)
        {
            var report = await _context.Reports
                .Include(r => r.ReporterUser)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report == null)
                return null;

            var targetDto = await BuildTargetDtoAsync(
       report.TargetType ?? "",
       report.TargetId ?? 0);

            return new ReportDetailDto
            {
                ReportId = report.ReportId,
                Reason = report.Reason ?? "",
                Status = report.Status ?? "",
                CreatedAt = report.CreatedAt ?? DateTime.Now,

                Reporter = new ReportReporterDto
                {
                    UserId = report.ReporterUser?.UserId ?? 0,
                    FullName = report.ReporterUser?.FullName ?? "",
                    Email = report.ReporterUser?.Email ?? "",
                    AvatarUrl = report.ReporterUser?.AvatarUrl ?? ""
                },

                Target = targetDto
            };
        }

        public async Task<bool> UpdateReportStatusAsync(
            int reportId,
            UpdateReportStatusDto dto)
        {
            var report = await _context.Reports
                .FirstOrDefaultAsync(x => x.ReportId == reportId);

            if (report == null)
                return false;

            report.Status = dto.Status;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ReportedPostDto>> GetReportedPostsAsync()
        {
            return await _context.Reports
                .Where(r => r.TargetType == "Post")
                .GroupBy(r => r.TargetId)
                .Select(g => new
                {
                    PostId = g.Key,
                    ReportCount = g.Count()
                })
                .Join(
                    _context.Posts.Include(p => p.User),
                    g => g.PostId,
                    p => p.PostId,
               (g, p) => new ReportedPostDto
               {
                   PostId = p.PostId,
                   Content = p.Content ?? "",
                   ImageUrl = p.ImageUrl,
                   UserId = p.UserId ?? 0,
                   UserName = p.User != null
        ? p.User.FullName
        : "",
                   ReportCount = g.ReportCount,
                   CreatedAt = p.CreatedAt ?? DateTime.Now
               })
                .OrderByDescending(x => x.ReportCount)
                .ToListAsync();
        }

        public async Task<List<ReportedCommentDto>> GetReportedCommentsAsync()
        {
            return await _context.Reports
                .Where(r => r.TargetType == "Comment")
                .GroupBy(r => r.TargetId)
                .Select(g => new
                {
                    CommentId = g.Key,
                    ReportCount = g.Count()
                })
                .Join(
                    _context.Comments.Include(c => c.User),
                    g => g.CommentId,
                    c => c.CommentId,
                   (g, c) => new ReportedCommentDto
                   {
                       CommentId = c.CommentId,
                       PostId = c.PostId ?? 0,
                       Content = c.Content ?? "",
                       UserId = c.UserId ?? 0,
                       UserName = c.User != null
        ? c.User.FullName
        : "",
                       ReportCount = g.ReportCount,
                       CreatedAt = c.CreatedAt ?? DateTime.Now
                   })
                .OrderByDescending(x => x.ReportCount)
                .ToListAsync();
        }

        private async Task<ReportTargetDto> BuildTargetDtoAsync(
            string targetType,
            int targetId)
        {
            switch (targetType)
            {
                case "Post":
                    {
                        var post = await _context.Posts
                            .Include(p => p.User)
                            .FirstOrDefaultAsync(p => p.PostId == targetId);

                        if (post == null)
                            break;

                        return new ReportTargetDto
                        {
                            TargetId = post.PostId,
                            TargetType = "Post",
                            Content = post.Content,
                            OwnerUserId = post.UserId ?? 0,
                            OwnerName = post.User.FullName
                        };
                    }

                case "Comment":
                    {
                        var comment = await _context.Comments
                            .Include(c => c.User)
                            .FirstOrDefaultAsync(c => c.CommentId == targetId);

                        if (comment == null)
                            break;

                        return new ReportTargetDto
                        {
                            TargetId = comment.CommentId,
                            TargetType = "Comment",
                            Content = comment.Content,
                            OwnerUserId = comment.UserId ?? 0,
                            OwnerName = comment.User.FullName
                        };
                    }

                case "User":
                    {
                        var user = await _context.Users
                            .FirstOrDefaultAsync(u => u.UserId == targetId);

                        if (user == null)
                            break;

                        return new ReportTargetDto
                        {
                            TargetId = user.UserId,
                            TargetType = "User",
                            Content = user.FullName,
                            OwnerUserId = user.UserId,
                            OwnerName = user.FullName
                        };
                    }
            }

            return new ReportTargetDto
            {
                TargetId = targetId,
                TargetType = targetType,
                Content = "Not Found",
                OwnerUserId = 0,
                OwnerName = "Unknown"
            };
        }
    }
}