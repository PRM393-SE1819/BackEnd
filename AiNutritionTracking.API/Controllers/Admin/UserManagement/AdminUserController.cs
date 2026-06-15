using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Admin.UserManagement;
using AiNutritionTracking.API.Services.Admin.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiNutritionTracking.API.Controllers.Admin.UserManagement
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUserController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet("api/admin/users")]
        public async Task<IActionResult> GetUsers([FromQuery] AdminUserQueryDto query)
        {
            var result = await _adminUserService.GetUsersAsync(query);
            return Ok(result);
        }

        [HttpGet("api/admin/users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _adminUserService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });
            return Ok(user);
        }

        [HttpPut("api/admin/users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
        {
            var success = await _adminUserService.UpdateUserStatusAsync(id, dto.Status);
            if (!success) return NotFound(new { message = "User not found." });
            return Ok(new { message = "User status updated successfully." });
        }

        [HttpPut("api/admin/users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            var success = await _adminUserService.UpdateUserRoleAsync(id, dto.RoleId);
            if (!success) return NotFound(new { message = "User or Role not found." });
            return Ok(new { message = "User role updated successfully." });
        }

        [HttpDelete("api/admin/users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _adminUserService.SoftDeleteUserAsync(id);
            if (!success) return NotFound(new { message = "User not found." });
            return Ok(new { message = "User deleted successfully." });
        }
    }
}