using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Admin.UserManagement;
using AiNutritionTracking.API.DTOs.Meal;

namespace AiNutritionTracking.API.Services.Admin.UserManagement
{
    public class AdminUserService : IAdminUserService
    {
        private readonly AinutritiontrackingContext _context;

        public AdminUserService(AinutritiontrackingContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<AdminUserItemDto>> GetUsersAsync(AdminUserQueryDto query)
        {
            var q = _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsDeleted != true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.ToLower();
                q = q.Where(u =>
                    u.Username.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s) ||
                    u.FullName.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
                q = q.Where(u => u.Status == query.Status);

            if (query.RoleId.HasValue)
                q = q.Where(u => u.RoleId == query.RoleId.Value);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(u => u.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(u => new AdminUserItemDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Status = u.Status,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.RoleName : null,
                    EmailVerified = u.EmailVerified,
                    IsDeleted = u.IsDeleted,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return new PagedResponse<AdminUserItemDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            };
        }

        public async Task<AdminUserItemDto?> GetUserByIdAsync(int id)
        {
            var u = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id && u.IsDeleted != true);

            if (u == null) return null;

            return new AdminUserItemDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Status = u.Status,
                RoleId = u.RoleId,
                RoleName = u.Role?.RoleName,
                EmailVerified = u.EmailVerified,
                IsDeleted = u.IsDeleted,
                CreatedAt = u.CreatedAt
            };
        }

        public async Task<bool> UpdateUserStatusAsync(int id, string status)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && u.IsDeleted != true);
            if (user == null) return false;

            user.Status = status;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(int id, int roleId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && u.IsDeleted != true);
            if (user == null) return false;

            var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == roleId);
            if (!roleExists) return false;

            user.RoleId = roleId;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteUserAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && u.IsDeleted != true);
            if (user == null) return false;

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}