using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Health;
using AiNutritionTracking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiNutritionTracking.API.Controllers
{
    [Route("api/health-profile")]
    [ApiController]
    [Authorize]
    public class HealthProfileController : ControllerBase
    {
        private readonly IHealthProfileService _healthProfileService;

        public HealthProfileController(IHealthProfileService healthProfileService)
        {
            _healthProfileService = healthProfileService;
        }

        // Helper Method để tự bóc tách Id của người dùng đang gọi API từ JWT Token
        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không tìm thấy thông tin định danh người dùng trong Token.");
        }

        // BẢN THÂN HỒ SƠ Y TẾ
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                int userId = GetCurrentUserId();
                var profile = await _healthProfileService.GetUserProfileAsync(userId);
                if (profile == null)
                    return NotFound(new { message = "Không tìm thấy hồ sơ người dùng." });

                return Ok(profile);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDTO request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _healthProfileService.UpdateUserProfileAsync(userId, request);

                if (result.Success)
                    return Ok(new { message = result.Message, data = result.Data });
                
                return BadRequest(new { message = result.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi cập nhật hồ sơ: " + ex.Message });
            }
        }

        
        // QUẢN LÝ BỆNH LÝ (HEALTH CONDITIONS)
        
        [HttpGet("conditions")]
        public async Task<IActionResult> GetHealthConditions()
        {
            try
            {
                int userId = GetCurrentUserId();
                var conditions = await _healthProfileService.GetHealthConditionsAsync(userId);
                return Ok(conditions);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("conditions")]
        public async Task<IActionResult> AddHealthCondition([FromBody] AddHealthConditionDTO request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _healthProfileService.AddHealthConditionAsync(userId, request);

                if (result.Success)
                    return Ok(new { message = result.Message, data = result.Data });
                return BadRequest(new { message = result.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpDelete("conditions/{id}")]
        public async Task<IActionResult> DeleteHealthCondition(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _healthProfileService.DeleteHealthConditionAsync(userId, id);

                if (result.Success) return Ok(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("conditions/{id}")]
        public async Task<IActionResult> UpdateHealthCondition(int id, [FromBody] UpdateHealthConditionDTO request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _healthProfileService.UpdateHealthConditionAsync(userId, id, request);

                if (result.Success) return Ok(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

       
        // QUẢN LÝ DỊ ỨNG (ALLERGIES)
        

        [HttpGet("allergies")]
        public async Task<IActionResult> GetAllergies()
        {
            try
            {
                int userId = GetCurrentUserId();
                var allergies = await _healthProfileService.GetAllergiesAsync(userId);
                return Ok(allergies);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("allergies")]
        public async Task<IActionResult> AddAllergy([FromBody] AddAllergyDTO request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _healthProfileService.AddAllergyAsync(userId, request);

                if (result.Success)
                    return Ok(new { message = result.Message, data = result.Data });
                return BadRequest(new { message = result.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpDelete("allergies/{id}")]
        public async Task<IActionResult> DeleteAllergy(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _healthProfileService.DeleteAllergyAsync(userId, id);

                if (result.Success) return Ok(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("allergies/{id}")]
        public async Task<IActionResult> UpdateAllergy(int id, [FromBody] AddAllergyDTO request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _healthProfileService.UpdateAllergyAsync(userId, id, request);

                if (result.Success) return Ok(new { message = result.Message });
                return BadRequest(new { message = result.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
