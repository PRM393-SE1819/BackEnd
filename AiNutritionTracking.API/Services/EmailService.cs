using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading;
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
            var host = _configuration["Smtp:Host"];
            var portStr = _configuration["Smtp:Port"] ?? "465"; // Mặc định dùng 465 cho SSL
            int port = int.Parse(portStr);
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"];
            var fromName = _configuration["Smtp:FromName"] ?? "AiNutritionTracking";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = bodyHtml };

            using var client = new SmtpClient();

            // Tăng timeout lên 20 giây cho môi trường cloud
            client.Timeout = 20000;

            try
            {
                using var cts = new CancellationTokenSource(20000);

                // Kết nối dùng SSL trực tiếp (Port 465) thay vì StartTls
                await client.ConnectAsync(host, port, SecureSocketOptions.SslOnConnect, cts.Token);
                await client.AuthenticateAsync(username, password, cts.Token);
                await client.SendAsync(message, cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService Error]: Gửi mail thất bại. Chi tiết: {ex.Message}");
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
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