namespace AiNutritionTracking.API.Services
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string toEmail, string fullName, string verificationUrl);
        Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink);
    }
}