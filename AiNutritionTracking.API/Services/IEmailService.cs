namespace AiNutritionTracking.API.Services
{
    public interface IEmailService
    {
        Task SendEmailVerificationOtpAsync(string toEmail, string fullName, string otpCode);
        Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink);
    }
}
