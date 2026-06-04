using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace AiNutritionTracking.API.DTOs.Auth
{

    public class RegisterRequestDTO
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$",
            ErrorMessage = "Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới, không có khoảng trắng.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [MaxLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; } = null!;
    }
    public class VerifyOtpRequestDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string OtpCode { get; set; } = null!;
    }
    public class ResendOtpRequestDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;
    }
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string Password { get; set; } = null!;
    }
    public class GoogleLoginRequestDTO
    {
        [Required]
        public string IdToken { get; set; } = null!;
    }

    public class LoginResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string? Token { get; set; }

    }
    public class GoogleLoginResponseDTO
    {
        public string AccessToken { get; set; } = null!;
        public int ExpiresInMinutes { get; set; }
    }
    public class RequestPasswordResetDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "Mã xác thực không được để trống.")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.")]
        public string NewPassword { get; set; } = null!;
    }
    public class AuthResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
    }
}

