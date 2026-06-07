using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Food;
using AiNutritionTracking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiNutritionTracking.API.Controllers
{
    [ApiController]
    public class FoodController : ControllerBase
    {
        private readonly IFoodService _foodService;

        public FoodController(IFoodService foodService)
        {
            _foodService = foodService;
        }

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không tìm th?y thông tin ??nh danh ng??i dùng trong Token.");
        }

        // Search Foods
        [HttpGet("api/foods")]
        public async Task<IActionResult> SearchFoods([FromQuery] FoodSearchRequestDto request)
        {
            var result = await _foodService.SearchFoodsAsync(request);
            return Ok(result);
        }

        // Get Food by Id
        [HttpGet("api/foods/{id}")]
        public async Task<IActionResult> GetFood(int id)
        {
            var food = await _foodService.GetFoodByIdAsync(id);
            if (food == null) return NotFound(new { message = "Food not found." });
            return Ok(food);
        }

        // Nutrition Details
        [HttpGet("api/foods/{id}/nutrition")]
        public async Task<IActionResult> GetFoodNutrition(int id)
        {
            var nutrition = await _foodService.GetFoodNutritionAsync(id);
            if (nutrition == null) return NotFound(new { message = "Food not found." });
            return Ok(nutrition);
        }

        // Barcode Scanning
        [HttpGet("api/foods/barcode/{barcode}")]
        public async Task<IActionResult> ScanBarcode(string barcode)
        {
            var result = await _foodService.ScanBarcodeAsync(barcode);
            if (!result.Found) return NotFound(new { message = "Barcode not found.", found = false });
            return Ok(result);
        }

        // Custom Foods
        [Authorize]
        [HttpPost("api/foods/custom")]
        public async Task<IActionResult> CreateCustomFood([FromBody] CreateCustomFoodDto request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _foodService.CreateCustomFoodAsync(userId, request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("api/foods/custom/{id}")]
        public async Task<IActionResult> UpdateCustomFood(int id, [FromBody] UpdateCustomFoodDto request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _foodService.UpdateCustomFoodAsync(userId, id, request);
                if (result == null) return NotFound(new { message = "Custom food not found or unauthorized." });
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("api/foods/custom/{id}")]
        public async Task<IActionResult> DeleteCustomFood(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var success = await _foodService.DeleteCustomFoodAsync(userId, id);
                if (!success) return NotFound(new { message = "Custom food not found or unauthorized." });
                return Ok(new { message = "Custom food deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // Favorite Foods
        [Authorize]
        [HttpPost("api/favorite-foods")]
        public async Task<IActionResult> AddFavoriteFood([FromBody] AddFavoriteFoodDto request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _foodService.AddFavoriteFoodAsync(userId, request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("api/favorite-foods")]
        public async Task<IActionResult> GetFavoriteFoods()
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _foodService.GetFavoriteFoodsAsync(userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("api/favorite-foods/{foodId}")]
        public async Task<IActionResult> RemoveFavoriteFood(int foodId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var success = await _foodService.RemoveFavoriteFoodAsync(userId, foodId);
                if (!success) return NotFound(new { message = "Favorite food not found." });
                return Ok(new { message = "Favorite food removed successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}