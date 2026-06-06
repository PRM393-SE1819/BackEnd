using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AiNutritionTracking.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task SendMailAsync(string toEmail, string fullName, string subject, string bodyHtml)
        {
            var apiKey = _configuration["ElasticEmail:ApiKey"];
            var fromEmail = _configuration["ElasticEmail:FromEmail"];

            using var http = new HttpClient();

            var payload = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("apikey", apiKey),
                new KeyValuePair<string, string>("from", fromEmail),
                new KeyValuePair<string, string>("fromName", "AiNutritionTracking"),
                new KeyValuePair<string, string>("to", toEmail),
                new KeyValuePair<string, string>("subject", subject),
                new KeyValuePair<string, string>("bodyHtml", bodyHtml),
                new KeyValuePair<string, string>("isTransactional", "true")
            });

            try
            {
                var response = await http.PostAsync("https://api.elasticemail.com/v2/email/send", payload);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"ElasticEmail error: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService Error]: Gửi mail thất bại. Chi tiết: {ex.Message}");
                throw;
            }
        }

        public async Task SendEmailVerificationOtpAsync(string toEmail, string fullName, string otpCode)
        {
            var body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px;'>
                        <h2>Xin chào {fullName},</h2>
                        <p>Dưới đây là mã OTP của bạn:</p>
                        <h1 style='color: #0078d4;'>{otpCode}</h1>
                        <p>Mã có hiệu lực trong 5 phút.</p>
                      </div>";
            await SendMailAsync(toEmail, fullName, "Mã xác thực tài khoản", body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink)
        {
            var body = $@"<div style='font-family: Arial, sans-serif; max-width:600px;'>
                        <h3>Xin chào {fullName},</h3>
                        <p>Nhấn vào link bên dưới để đặt lại mật khẩu:</p>
                        <a href='{resetLink}'>Đặt lại mật khẩu</a>
                      </div>";
            await SendMailAsync(toEmail, fullName, "Yêu cầu đặt lại mật khẩu", body);
        }
    }
}