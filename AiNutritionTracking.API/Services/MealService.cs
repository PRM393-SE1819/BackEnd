using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Meal;
using AiNutritionTracking.API.Models;

namespace AiNutritionTracking.API.Services
{
    public class MealService : IMealService
    {
        private readonly AinutritiontrackingContext _context;

        public MealService(AinutritiontrackingContext context)
        {
            _context = context;
        }

        private MealDto MapToMealDto(Meal meal)
        {
            return new MealDto
            {
                MealId = meal.MealId,
                MealType = meal.MealType ?? string.Empty,
                MealDate = meal.MealDate.HasValue ? meal.MealDate.Value.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                Notes = meal.Notes,
                TotalCalories = (float)(meal.TotalCalories ?? 0),
                Items = meal.MealItems.Select(mi => new MealItemDto
                {
                    MealItemId = mi.MealItemId,
                    FoodId = mi.FoodId ?? 0,
                    FoodName = mi.Food?.Name ?? string.Empty,
                    Quantity = (float)(mi.Quantity ?? 0),
                    Calories = (float)(mi.Calories ?? 0),
                    Protein = (float)(mi.Protein ?? 0),
                    Carbs = (float)(mi.Carbs ?? 0),
                    Fat = (float)(mi.Fat ?? 0)
                }).ToList()
            };
        }

        public async Task<MealDto> AddMealAsync(int userId, CreateMealDto request)
        {
            var meal = new Meal
            {
                UserId = userId,
                MealType = request.MealType,
                MealDate = DateOnly.FromDateTime(request.MealDate),
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                TotalCalories = 0
            };

            double totalCals = 0;

            foreach (var item in request.Items)
            {
                var food = await _context.Foods.FindAsync(item.FoodId);
                if (food != null)
                {
                    var cals = (food.Calories * item.Quantity);
                    var mealItem = new MealItem
                    {
                        FoodId = item.FoodId,
                        Quantity = item.Quantity,
                        Calories = cals,
                        Protein = food.Protein * item.Quantity,
                        Carbs = food.Carbs * item.Quantity,
                        Fat = food.Fat * item.Quantity
                    };
                    meal.MealItems.Add(mealItem);
                    totalCals += cals;
                }
            }

            meal.TotalCalories = totalCals;

            _context.Meals.Add(meal);
            await _context.SaveChangesAsync();

            var createdMeal = await _context.Meals
                .Include(m => m.MealItems)
                .ThenInclude(mi => mi.Food)
                .FirstOrDefaultAsync(m => m.MealId == meal.MealId);

            return MapToMealDto(createdMeal!);
        }

        public async Task<MealDto?> UpdateMealAsync(int userId, int mealId, UpdateMealDto request)
        {
            var meal = await _context.Meals
                .Include(m => m.MealItems)
                .FirstOrDefaultAsync(m => m.MealId == mealId && m.UserId == userId);

            if (meal == null) return null;

            meal.MealType = request.MealType;
            meal.Notes = request.Notes;

            _context.MealItems.RemoveRange(meal.MealItems);
            meal.MealItems.Clear();

            double totalCals = 0;
            foreach (var item in request.Items)
            {
                var food = await _context.Foods.FindAsync(item.FoodId);
                if (food != null)
                {
                    var cals = (food.Calories * item.Quantity);
                    var mealItem = new MealItem
                    {
                        FoodId = item.FoodId,
                        Quantity = item.Quantity,
                        Calories = cals,
                        Protein = food.Protein * item.Quantity,
                        Carbs = food.Carbs * item.Quantity,
                        Fat = food.Fat * item.Quantity
                    };
                    meal.MealItems.Add(mealItem);
                    totalCals += cals;
                }
            }

            meal.TotalCalories = totalCals;

            await _context.SaveChangesAsync();

            var updatedMeal = await _context.Meals
                .Include(m => m.MealItems)
                .ThenInclude(mi => mi.Food)
                .FirstOrDefaultAsync(m => m.MealId == mealId);

            return MapToMealDto(updatedMeal!);
        }

        public async Task<bool> DeleteMealAsync(int userId, int mealId)
        {
            var meal = await _context.Meals.FirstOrDefaultAsync(m => m.MealId == mealId && m.UserId == userId);
            if (meal == null) return false;

            _context.Meals.Remove(meal);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MealDto?> GetMealDetailAsync(int userId, int mealId)
        {
            var meal = await _context.Meals
                .Include(m => m.MealItems)
                .ThenInclude(mi => mi.Food)
                .FirstOrDefaultAsync(m => m.MealId == mealId && m.UserId == userId);

            if (meal == null) return null;

            return MapToMealDto(meal);
        }

        public async Task<PagedResponse<MealDto>> GetMealHistoryAsync(int userId, int page, int pageSize, DateTime? date, string? mealType)
        {
            var query = _context.Meals
                .Include(m => m.MealItems)
                .ThenInclude(mi => mi.Food)
                .Where(m => m.UserId == userId)
                .AsQueryable();

            if (date.HasValue)
            {
                var dateOnly = DateOnly.FromDateTime(date.Value);
                query = query.Where(m => m.MealDate == dateOnly);
            }

            if (!string.IsNullOrEmpty(mealType))
            {
                query = query.Where(m => m.MealType != null && m.MealType.ToLower() == mealType.ToLower());
            }

            int totalItems = await query.CountAsync();

            var meals = await query
                .OrderByDescending(m => m.MealDate)
                .ThenByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<MealDto>
            {
                Items = meals.Select(MapToMealDto).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public async Task<DailyCaloriesSummaryDto> GetDailyCaloriesSummaryAsync(int userId, DateTime date)
        {
            var dateOnly = DateOnly.FromDateTime(date);

            var user = await _context.Users.FindAsync(userId);
            int calTarget = user?.CaloriesTarget ?? 2000;

            var meals = await _context.Meals
                .Include(m => m.MealItems)
                .Where(m => m.UserId == userId && m.MealDate == dateOnly)
                .ToListAsync();

            var allItems = meals.SelectMany(m => m.MealItems).ToList();

            float caloriesConsumed = (float)allItems.Sum(i => i.Calories ?? 0);
            float protein = (float)allItems.Sum(i => i.Protein ?? 0);
            float carbs = (float)allItems.Sum(i => i.Carbs ?? 0);
            float fat = (float)allItems.Sum(i => i.Fat ?? 0);

            return new DailyCaloriesSummaryDto
            {
                Date = date,
                CaloriesConsumed = caloriesConsumed,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                CaloriesTarget = calTarget,
                RemainingCalories = calTarget - caloriesConsumed
            };
        }
    }
}