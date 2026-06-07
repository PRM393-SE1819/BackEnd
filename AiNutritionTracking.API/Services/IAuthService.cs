using System;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Auth;
namespace AiNutritionTracking.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request);
        Task<AuthResponseDTO> VerifyEmailAsync(VerifyEmailRequestDTO request);
        Task<AuthResponseDTO> ResendVerificationEmailAsync(ResendVerificationEmailRequestDTO request);
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<GoogleLoginResponseDTO> GoogleLoginAsync(GoogleLoginRequestDTO request);
        Task RevokeTokenAsync(string jti, DateTime expiresAt);
        Task<AuthResponseDTO> RequestPasswordResetAsync(RequestPasswordResetDTO request);
        Task<AuthResponseDTO> ResetPasswordAsync(ResetPasswordDTO request);
    }
}