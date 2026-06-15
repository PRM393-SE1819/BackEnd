using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Admin.Dashboard;
using AiNutritionTracking.API.Services.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiNutritionTracking.API.Services.Admin
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AinutritiontrackingContext _context;

        public AdminDashboardService(
            AinutritiontrackingContext context)
        {
            _context = context;
        }
        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            return new DashboardSummaryDto
            {
                TotalUsers = await _context.Users.CountAsync(),

                ActiveUsers = await _context.Users
                    .CountAsync(x => x.Status == "Active"),

                BannedUsers = await _context.Users
                    .CountAsync(x => x.Status == "Banned"),

                TotalFoods = await _context.Foods.CountAsync(),

                VerifiedFoods = await _context.Foods
                    .CountAsync(x => x.IsVerified == true),

                TotalPosts = await _context.Posts
                    .CountAsync(x => x.IsDeleted != true),

                PendingReports = await _context.Reports
                    .CountAsync(x => x.Status == "Pending"),

                TotalAIRequests = await _context.Airequests
                    .CountAsync()
            };
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            var today = DateTime.UtcNow.Date;

            return new UserStatisticsDto
            {
                TotalUsers = await _context.Users.CountAsync(),

                NewUsersToday = await _context.Users
                    .CountAsync(x =>
                        x.CreatedAt.HasValue &&
                        x.CreatedAt.Value.Date == today),

                NewUsersThisWeek = await _context.Users
                    .CountAsync(x =>
                        x.CreatedAt.HasValue &&
                        x.CreatedAt.Value >= today.AddDays(-7)),

                NewUsersThisMonth = await _context.Users
                    .CountAsync(x =>
                        x.CreatedAt.HasValue &&
                        x.CreatedAt.Value >= today.AddMonths(-1))
            };
        }

        public async Task<FoodStatisticsDto> GetFoodStatisticsAsync()
        {
            return new FoodStatisticsDto
            {
                TotalFoods = await _context.Foods.CountAsync(),

                VerifiedFoods = await _context.Foods
    .CountAsync(x => x.IsVerified == true),

                UnverifiedFoods = await _context.Foods
    .CountAsync(x => !x.IsVerified == true),

                DeletedFoods = await _context.Foods
    .CountAsync(x => x.IsDeleted == true)
            };
        }

        public async Task<CommunityStatisticsDto> GetCommunityStatisticsAsync()
        {
            return new CommunityStatisticsDto
            {
                TotalPosts = await _context.Posts
                   .CountAsync(x => x.IsDeleted != true),

                TotalComments = await _context.Comments
                    .CountAsync(),

                PendingReports = await _context.Reports
                    .CountAsync(x => x.Status == "Pending"),

                ApprovedReports = await _context.Reports
                    .CountAsync(x => x.Status == "Approved"),

                RejectedReports = await _context.Reports
                    .CountAsync(x => x.Status == "Rejected")
            };
        }

        public async Task<List<UserGrowthDto>> GetUserGrowthAsync()
        {
            var last30Days = DateTime.UtcNow.Date.AddDays(-30);

            var data = await _context.Users
                .Where(x => x.CreatedAt != null &&
                            x.CreatedAt >= last30Days)
                .GroupBy(x => x.CreatedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Users = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return data.Select(x => new UserGrowthDto
            {
                Date = x.Date.ToString("yyyy-MM-dd"),
                Users = x.Users
            }).ToList();
        }
        public async Task<List<ReportTrendDto>> GetReportTrendsAsync()
        {
            var last30Days = DateTime.UtcNow.Date.AddDays(-30);

            var data = await _context.Reports
                .Where(x => x.CreatedAt != null &&
                            x.CreatedAt >= last30Days)
                .GroupBy(x => x.CreatedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Reports = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return data.Select(x => new ReportTrendDto
            {
                Date = x.Date.ToString("yyyy-MM-dd"),
                Reports = x.Reports
            }).ToList();
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            return new DashboardDto
            {
                Summary = await GetSummaryAsync(),

                UserStatistics = await GetUserStatisticsAsync(),

                FoodStatistics = await GetFoodStatisticsAsync(),

                CommunityStatistics = await GetCommunityStatisticsAsync(),

                UserGrowth = await GetUserGrowthAsync(),

                ReportTrends = await GetReportTrendsAsync()
            };
        }
    }
}