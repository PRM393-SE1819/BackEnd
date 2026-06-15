using System;

namespace AiNutritionTracking.API.DTOs.Admin.UserManagement
{
    public class AdminUserQueryDto
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public int? RoleId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminUserItemDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Status { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool? EmailVerified { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class UpdateUserStatusDto
    {
        public string Status { get; set; } = null!;
    }

    public class UpdateUserRoleDto
    {
        public int RoleId { get; set; }
    }
}