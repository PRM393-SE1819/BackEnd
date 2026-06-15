using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Admin.FoodManagement;
using AiNutritionTracking.API.Services.Admin.FoodManagement;
using AiNutritionTracking.API.Services.Cloudinary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiNutritionTracking.API.Controllers.Admin.FoodManagement
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminFoodController : ControllerBase
    {
        private readonly IAdminFoodService _adminFoodService;
        private readonly ICloudinaryService _cloudinaryService;

        public AdminFoodController(IAdminFoodService adminFoodService, ICloudinaryService cloudinaryService)
        {
            _adminFoodService = adminFoodService;
            _cloudinaryService = cloudinaryService;
        }

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirst("id")?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out int userId))
                return userId;
            throw new UnauthorizedAccessException("Không tìm thấy thông tin định danh người dùng trong Token.");
        }

        [HttpPost("api/admin/upload-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageDto request)
        {
            var file = request.File;

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Không có file." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Chỉ chấp nhận file JPG, PNG, WEBP." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "File không được vượt quá 5MB." });

            var url = await _cloudinaryService.UploadImageAsync(file, "foods");
            if (url == null)
                return StatusCode(500, new { message = "Upload ảnh thất bại." });

            return Ok(new { imageUrl = url });
        }

        [HttpGet("api/admin/foods")]
        public async Task<IActionResult> GetFoods([FromQuery] AdminFoodQueryDto query)
        {
            var result = await _adminFoodService.GetFoodsAsync(query);
            return Ok(result);
        }

        [HttpGet("api/admin/foods/{id}")]
        public async Task<IActionResult> GetFoodById(int id)
        {
            var food = await _adminFoodService.GetFoodByIdAsync(id);
            if (food == null) return NotFound(new { message = "Food not found." });
            return Ok(food);
        }

        [HttpPost("api/admin/foods")]
        public async Task<IActionResult> CreateFood([FromBody] AdminCreateFoodDto dto)
        {
            try
            {
                int adminId = GetCurrentUserId();
                var result = await _adminFoodService.CreateFoodAsync(adminId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("api/admin/foods/{id}")]
        public async Task<IActionResult> UpdateFood(int id, [FromBody] AdminCreateFoodDto dto)
        {
            try
            {
                int adminId = GetCurrentUserId();
                var result = await _adminFoodService.UpdateFoodAsync(adminId, id, dto);
                if (result == null) return NotFound(new { message = "Food not found." });
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("api/admin/foods/{id}/verify")]
        public async Task<IActionResult> VerifyFood(int id, [FromQuery] bool isVerified = true)
        {
            var success = await _adminFoodService.VerifyFoodAsync(id, isVerified);
            if (!success) return NotFound(new { message = "Food not found." });
            return Ok(new { message = $"Food IsVerified set to {isVerified}." });
        }

        [HttpPut("api/admin/foods/{id}/status")]
        public async Task<IActionResult> UpdateFoodStatus(int id, [FromBody] AdminUpdateFoodStatusDto dto)
        {
            var success = await _adminFoodService.UpdateFoodStatusAsync(id, dto.Status);
            if (!success) return NotFound(new { message = "Food not found." });
            return Ok(new { message = "Food status updated successfully." });
        }

        [HttpDelete("api/admin/foods/{id}")]
        public async Task<IActionResult> DeleteFood(int id)
        {
            var success = await _adminFoodService.SoftDeleteFoodAsync(id);
            if (!success) return NotFound(new { message = "Food not found." });
            return Ok(new { message = "Food deleted successfully." });
        }
    }
}