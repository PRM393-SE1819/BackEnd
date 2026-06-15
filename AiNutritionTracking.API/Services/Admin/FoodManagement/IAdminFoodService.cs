using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Admin.FoodManagement;
using AiNutritionTracking.API.DTOs.Meal;

namespace AiNutritionTracking.API.Services.Admin.FoodManagement
{
    public interface IAdminFoodService
    {
        Task<PagedResponse<AdminFoodItemDto>> GetFoodsAsync(AdminFoodQueryDto query);
        Task<AdminFoodDetailDto?> GetFoodByIdAsync(int id);
        Task<AdminFoodDetailDto> CreateFoodAsync(int adminUserId, AdminCreateFoodDto dto);
        Task<AdminFoodDetailDto?> UpdateFoodAsync(int adminUserId, int foodId, AdminCreateFoodDto dto);
        Task<bool> VerifyFoodAsync(int foodId, bool isVerified);
        Task<bool> UpdateFoodStatusAsync(int foodId, string status);
        Task<bool> SoftDeleteFoodAsync(int foodId);
    }
}