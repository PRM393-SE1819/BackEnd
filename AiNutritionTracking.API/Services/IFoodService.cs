using System.Collections.Generic;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Food;

namespace AiNutritionTracking.API.Services
{
    public interface IFoodService
    {
        Task<FoodSearchResponseDto> SearchFoodsAsync(FoodSearchRequestDto request);
        Task<FoodDetailDto?> GetFoodByIdAsync(int id);
        Task<FoodDetailDto?> GetFoodNutritionAsync(int id);
        Task<BarcodeScanResponseDto> ScanBarcodeAsync(string barcode);
        Task<CustomFoodDto> CreateCustomFoodAsync(int userId, CreateCustomFoodDto request);
        Task<CustomFoodDto?> UpdateCustomFoodAsync(int userId, int foodId, UpdateCustomFoodDto request);
        Task<bool> DeleteCustomFoodAsync(int userId, int foodId);

        Task<FavoriteFoodDto> AddFavoriteFoodAsync(int userId, AddFavoriteFoodDto request);
        Task<List<FavoriteFoodDto>> GetFavoriteFoodsAsync(int userId);
        Task<bool> RemoveFavoriteFoodAsync(int userId, int foodId);
    }
}