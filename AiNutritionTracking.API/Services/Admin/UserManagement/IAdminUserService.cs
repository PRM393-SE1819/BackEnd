using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Admin.UserManagement;
using AiNutritionTracking.API.DTOs.Meal;

namespace AiNutritionTracking.API.Services.Admin.UserManagement
{
    public interface IAdminUserService
    {
        Task<PagedResponse<AdminUserItemDto>> GetUsersAsync(AdminUserQueryDto query);
        Task<AdminUserItemDto?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserStatusAsync(int id, string status);
        Task<bool> UpdateUserRoleAsync(int id, int roleId);
        Task<bool> SoftDeleteUserAsync(int id);
    }
}