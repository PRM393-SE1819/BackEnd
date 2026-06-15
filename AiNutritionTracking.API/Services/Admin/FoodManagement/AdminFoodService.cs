using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Admin.FoodManagement;
using AiNutritionTracking.API.DTOs.Meal;
using AiNutritionTracking.API.Models;

namespace AiNutritionTracking.API.Services.Admin.FoodManagement
{
    public class AdminFoodService : IAdminFoodService
    {
        private readonly AinutritiontrackingContext _context;

        public AdminFoodService(AinutritiontrackingContext context)
        {
            _context = context;
        }

        private AdminFoodDetailDto MapToDetail(Food f, string? createdByUsername, string? updatedByUsername)
            => new AdminFoodDetailDto
            {
                FoodId = f.FoodId,
                Name = f.Name,
                Description = f.Description,
                Calories = f.Calories,
                Protein = f.Protein,
                Carbs = f.Carbs,
                Fat = f.Fat,
                Fiber = f.Fiber,
                Sugar = f.Sugar,
                Sodium = f.Sodium,
                ServingSize = f.ServingSize,
                Barcode = f.Barcode,
                ImageUrl = f.ImageUrl,
                FoodType = f.FoodType,
                IsVerified = f.IsVerified,
                Status = f.Status,
                IsDeleted = f.IsDeleted,
                CreatedBy = f.CreatedBy,
                CreatedByUsername = createdByUsername,
                UpdatedBy = f.UpdatedBy,
                UpdatedByUsername = updatedByUsername,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            };

        public async Task<PagedResponse<AdminFoodItemDto>> GetFoodsAsync(AdminFoodQueryDto query)
        {
            var q = _context.Foods
                .Include(f => f.CreatedByNavigation)
                .Where(f => f.IsDeleted != true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.ToLower();
                q = q.Where(f => f.Name.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
                q = q.Where(f => f.Status == query.Status);

            if (!string.IsNullOrWhiteSpace(query.FoodType))
                q = q.Where(f => f.FoodType == query.FoodType);

            if (query.IsVerified.HasValue)
                q = q.Where(f => f.IsVerified == query.IsVerified.Value);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(f => f.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(f => new AdminFoodItemDto
                {
                    FoodId = f.FoodId,
                    Name = f.Name,
                    Description = f.Description,
                    Calories = f.Calories,
                    Protein = f.Protein,
                    Carbs = f.Carbs,
                    Fat = f.Fat,
                    FoodType = f.FoodType,
                    IsVerified = f.IsVerified,
                    Status = f.Status,
                    IsDeleted = f.IsDeleted,
                    CreatedBy = f.CreatedBy,
                    CreatedByUsername = f.CreatedByNavigation != null ? f.CreatedByNavigation.Username : null,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            return new PagedResponse<AdminFoodItemDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            };
        }

        public async Task<AdminFoodDetailDto?> GetFoodByIdAsync(int id)
        {
            var f = await _context.Foods
                .Include(f => f.CreatedByNavigation)
                .Include(f => f.UpdatedByNavigation)
                .FirstOrDefaultAsync(f => f.FoodId == id && f.IsDeleted != true);

            if (f == null) return null;

            return MapToDetail(f, f.CreatedByNavigation?.Username, f.UpdatedByNavigation?.Username);
        }

        public async Task<AdminFoodDetailDto> CreateFoodAsync(int adminUserId, AdminCreateFoodDto dto)
        {
            var food = new Food
            {
                Name = dto.Name,
                Description = dto.Description,
                Calories = dto.Calories ?? 0,
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
                IsVerified = true,
                Status = "Active",
                IsDeleted = false,
                CreatedBy = adminUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Foods.Add(food);
            await _context.SaveChangesAsync();

            var adminUser = await _context.Users.FindAsync(adminUserId);
            return MapToDetail(food, adminUser?.Username, null);
        }

        public async Task<AdminFoodDetailDto?> UpdateFoodAsync(int adminUserId, int foodId, AdminCreateFoodDto dto)
        {
            var food = await _context.Foods
                .Include(f => f.CreatedByNavigation)
                .FirstOrDefaultAsync(f => f.FoodId == foodId && f.IsDeleted != true);

            if (food == null) return null;

            food.Name = dto.Name ?? food.Name;
            food.Description = dto.Description ?? food.Description;
            food.Calories = dto.Calories.HasValue ? dto.Calories.Value : food.Calories;
            food.Protein = dto.Protein ?? food.Protein;
            food.Carbs = dto.Carbs ?? food.Carbs;
            food.Fat = dto.Fat ?? food.Fat;
            food.Fiber = dto.Fiber ?? food.Fiber;
            food.Sugar = dto.Sugar ?? food.Sugar;
            food.Sodium = dto.Sodium ?? food.Sodium;
            food.ServingSize = dto.ServingSize ?? food.ServingSize;
            food.Barcode = dto.Barcode ?? food.Barcode;
            food.ImageUrl = dto.ImageUrl ?? food.ImageUrl;
            food.FoodType = dto.FoodType ?? food.FoodType;
            food.UpdatedBy = adminUserId;
            food.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var adminUser = await _context.Users.FindAsync(adminUserId);
            return MapToDetail(food, food.CreatedByNavigation?.Username, adminUser?.Username);
        }

        public async Task<bool> VerifyFoodAsync(int foodId, bool isVerified)
        {
            var food = await _context.Foods
                .FirstOrDefaultAsync(f => f.FoodId == foodId && f.IsDeleted != true);
            if (food == null) return false;

            food.IsVerified = isVerified;
            food.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateFoodStatusAsync(int foodId, string status)
        {
            var food = await _context.Foods
                .FirstOrDefaultAsync(f => f.FoodId == foodId && f.IsDeleted != true);
            if (food == null) return false;

            food.Status = status;
            food.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteFoodAsync(int foodId)
        {
            var food = await _context.Foods
                .FirstOrDefaultAsync(f => f.FoodId == foodId && f.IsDeleted != true);
            if (food == null) return false;

            food.IsDeleted = true;
            food.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

