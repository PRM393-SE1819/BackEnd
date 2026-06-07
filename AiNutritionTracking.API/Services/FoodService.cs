using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Food;
using AiNutritionTracking.API.Models;

namespace AiNutritionTracking.API.Services
{
    public class FoodService : IFoodService
    {
        private readonly AinutritiontrackingContext _context;

        public FoodService(AinutritiontrackingContext context)
        {
            _context = context;
        }

        private FoodDetailDto MapToFoodDetailDto(Food food)
        {
            return new FoodDetailDto
            {
                FoodId = food.FoodId,
                Name = food.Name,
                Description = food.Description,
                Calories = food.Calories,
                Protein = food.Protein,
                Carbs = food.Carbs,
                Fat = food.Fat,
                Fiber = food.Fiber,
                Sugar = food.Sugar,
                Sodium = food.Sodium,
                ServingSize = food.ServingSize,
                Barcode = food.Barcode,
                ImageUrl = food.ImageUrl,
                FoodType = food.FoodType,
                IsVerified = food.IsVerified
            };
        }

        public async Task<FoodSearchResponseDto> SearchFoodsAsync(FoodSearchRequestDto request)
        {
            var query = _context.Foods.Where(f => f.IsDeleted != true).AsQueryable();

            if (!string.IsNullOrEmpty(request.Query))
            {
                query = query.Where(f => f.Name.ToLower().Contains(request.Query.ToLower()));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

            var foods = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new FoodSearchResponseDto
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = request.Page,
                Items = foods.Select(MapToFoodDetailDto).ToList()
            };
        }

        public async Task<FoodDetailDto?> GetFoodByIdAsync(int id)
        {
            var food = await _context.Foods.FirstOrDefaultAsync(f => f.FoodId == id && f.IsDeleted != true);
            if (food == null) return null;
            return MapToFoodDetailDto(food);
        }

        public async Task<FoodDetailDto?> GetFoodNutritionAsync(int id)
        {
            return await GetFoodByIdAsync(id);
        }

        public async Task<BarcodeScanResponseDto> ScanBarcodeAsync(string barcode)
        {
            var food = await _context.Foods.FirstOrDefaultAsync(f => f.Barcode == barcode && f.IsDeleted != true);
            if (food == null)
            {
                return new BarcodeScanResponseDto { Found = false, Food = null };
            }
            return new BarcodeScanResponseDto { Found = true, Food = MapToFoodDetailDto(food) };
        }

        public async Task<CustomFoodDto> CreateCustomFoodAsync(int userId, CreateCustomFoodDto request)
        {
            var food = new Food
            {
                Name = request.Name,
                Description = request.Description,
                Calories = request.Calories,
                Protein = request.Protein,
                Carbs = request.Carbs,
                Fat = request.Fat,
                ServingSize = request.ServingSize,
                Barcode = request.Barcode,
                FoodType = "Custom",
                IsVerified = false,
                IsDeleted = false,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Foods.Add(food);
            await _context.SaveChangesAsync();

            var dto = MapToFoodDetailDto(food);
            return new CustomFoodDto
            {
                FoodId = dto.FoodId,
                Name = dto.Name,
                Description = dto.Description,
                Calories = dto.Calories,
                Protein = dto.Protein,
                Carbs = dto.Carbs,
                Fat = dto.Fat,
                Fiber = dto.Fiber,
                Sugar = dto.Sugar,
                Sodium = dto.Sodium,
                ServingSize = dto.ServingSize,
                Barcode = dto.Barcode,
                ImageUrl = dto.ImageUrl,
                FoodType = dto.FoodType,
                IsVerified = dto.IsVerified
            };
        }

        public async Task<CustomFoodDto?> UpdateCustomFoodAsync(int userId, int foodId, UpdateCustomFoodDto request)
        {
            var food = await _context.Foods.FirstOrDefaultAsync(f => f.FoodId == foodId && f.CreatedBy == userId && f.FoodType == "Custom" && f.IsDeleted != true);
            if (food == null) return null;

            food.Name = request.Name;
            food.Description = request.Description;
            food.Calories = request.Calories;
            food.Protein = request.Protein;
            food.Carbs = request.Carbs;
            food.Fat = request.Fat;
            food.ServingSize = request.ServingSize;
            food.Barcode = request.Barcode;
            food.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = MapToFoodDetailDto(food);
            return new CustomFoodDto
            {
                FoodId = dto.FoodId,
                Name = dto.Name,
                Description = dto.Description,
                Calories = dto.Calories,
                Protein = dto.Protein,
                Carbs = dto.Carbs,
                Fat = dto.Fat,
                Fiber = dto.Fiber,
                Sugar = dto.Sugar,
                Sodium = dto.Sodium,
                ServingSize = dto.ServingSize,
                Barcode = dto.Barcode,
                ImageUrl = dto.ImageUrl,
                FoodType = dto.FoodType,
                IsVerified = dto.IsVerified
            };
        }

        public async Task<bool> DeleteCustomFoodAsync(int userId, int foodId)
        {
            var food = await _context.Foods.FirstOrDefaultAsync(f => f.FoodId == foodId && f.CreatedBy == userId && f.FoodType == "Custom");
            if (food == null) return false;

            food.IsDeleted = true;
            food.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FavoriteFoodDto> AddFavoriteFoodAsync(int userId, AddFavoriteFoodDto request)
        {
            var existing = await _context.FavoriteFoods.FirstOrDefaultAsync(f => f.UserId == userId && f.FoodId == request.FoodId);
            if (existing != null)
            {
                var fdetail = await GetFoodByIdAsync(request.FoodId);
                return new FavoriteFoodDto { FavoriteFoodId = existing.FavoriteFoodId, FoodId = existing.FoodId ?? request.FoodId, CreatedAt = existing.CreatedAt, Food = fdetail };
            }

            var favorite = new FavoriteFood
            {
                UserId = userId,
                FoodId = request.FoodId,
                CreatedAt = DateTime.UtcNow
            };
            _context.FavoriteFoods.Add(favorite);
            await _context.SaveChangesAsync();

            var foodDetail = await GetFoodByIdAsync(request.FoodId);
            return new FavoriteFoodDto
            {
                FavoriteFoodId = favorite.FavoriteFoodId,
                FoodId = favorite.FoodId ?? request.FoodId,
                CreatedAt = favorite.CreatedAt,
                Food = foodDetail
            };
        }

        public async Task<List<FavoriteFoodDto>> GetFavoriteFoodsAsync(int userId)
        {
            var favorites = await _context.FavoriteFoods
                .Include(f => f.Food)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            return favorites.Select(f => new FavoriteFoodDto
            {
                FavoriteFoodId = f.FavoriteFoodId,
                FoodId = f.FoodId ?? 0,
                CreatedAt = f.CreatedAt,
                Food = f.Food != null && f.Food.IsDeleted != true ? MapToFoodDetailDto(f.Food) : null
            }).Where(d => d.Food != null).ToList();
        }

        public async Task<bool> RemoveFavoriteFoodAsync(int userId, int foodId)
        {
            var favorite = await _context.FavoriteFoods.FirstOrDefaultAsync(f => f.UserId == userId && f.FoodId == foodId);
            if (favorite == null) return false;

            _context.FavoriteFoods.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}