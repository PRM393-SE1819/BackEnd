using System.IdentityModel.Tokens.Jwt;
using AiNutritionTracking.API.DTOs.Auth;
using AiNutritionTracking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiNutritionTracking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            var result = await _authService.RegisterAsync(request);
            if (result.Success) return Ok(new { message = result.Message });
            return BadRequest(new { message = result.Message });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDTO request)
        {
            var result = await _authService.VerifyEmailAsync(request);
            if (result.Success) return Ok(new { message = result.Message });
            return BadRequest(new { message = result.Message });
        }

        [HttpPost("resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequestDTO request)
        {
            var result = await _authService.ResendVerificationEmailAsync(request);
            if (result.Success) return Ok(new { message = result.Message });
            return BadRequest(new { message = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var result = await _authService.LoginAsync(request);
            if (result.Success) return Ok(new { token = result.Token, message = result.Message });
            return BadRequest(new { message = result.Message });
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDTO request)
        {
            try
            {
                var result = await _authService.GoogleLoginAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(expClaim))
            {
                return BadRequest(new { message = "Không thể xác định thông tin token. Vui lòng đăng nhập lại và thử lại." });
            }

            if (!long.TryParse(expClaim, out var expSeconds))
            {
                return BadRequest(new { message = "Token không hợp lệ." });
            }

            var exp = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
            await _authService.RevokeTokenAsync(jti, exp);

            return Ok(new { message = "Đăng xuất thành công." });
        }

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDTO request)
        {
            var result = await _authService.RequestPasswordResetAsync(request);
            if (result.Success) return Ok(new { message = result.Message });
            return BadRequest(new { message = result.Message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            if (result.Success) return Ok(new { message = result.Message });
            return BadRequest(new { message = result.Message });
        }
    }
}