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

        public async Task SendEmailVerificationOtpAsync(string toEmail, string fullName, string otpCode)
        {
            var host = _configuration["Smtp:Host"];
            var portStr = _configuration["Smtp:Port"];
            int port = int.TryParse(portStr, out int parsedPort) ? parsedPort : 587; // Không lo bị nổ crash nếu thiếu Port
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"];
            var fromName = _configuration["Smtp:FromName"] ?? "AiNutritionTracking";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = "Mã xác thực tài khoản - AiNutritionTracking";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Xin chào {fullName},</h2>
                        <p>Cảm ơn bạn đã đăng ký tài khoản tại hệ thống của chúng tôi.</p>
                        <p>Dưới đây là mã xác thực (OTP) của bạn. Vui lòng nhập mã này vào ứng dụng để hoàn tất việc đăng ký:</p>
                        <div style='background-color: #f4f4f4; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #333;'>
                            {otpCode}
                        </div>
                        <p style='color: red; font-size: 14px;'>*Mã OTP này sẽ tự động hết hạn sau 5 phút.</p>
                        <p>Nếu bạn không thực hiện việc đăng ký này, vui lòng bỏ qua email.</p>
                        <br/>
                        <p>Trân trọng,<br/>Đội ngũ AiNutritionTracking</p>
                    </div>"
            };

            using var client = new SmtpClient();

            // 🚨 SỬA CHÍ MẠNG 1: Đặt giới hạn Timeout kết nối tối đa là 10 giây
            client.Timeout = 10000;

            try
            {
                // Sử dụng CancellationTokenSource để ép hủy Task nếu Mail Server phản hồi quá lâu
                using var cts = new CancellationTokenSource(10000);

                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cts.Token);
                await client.AuthenticateAsync(username, password, cts.Token);
                await client.SendAsync(message, cts.Token);
            }
            catch (Exception ex)
            {
                // 🚨 SỬA CHÍ MẠNG 2: Ghi log lỗi ra Render chứ không để nó tự treo ngậm ngùi
                Console.WriteLine($"[EmailService Error]: Gửi mail OTP thất bại. Chi tiết: {ex.Message}");
                throw; // Ném lỗi ra ngoài để tầng Controller biết đường xử lý (trả về lỗi rõ ràng thay vì loading vô tận)
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink)
        {
            var host = _configuration["Smtp:Host"];
            var portStr = _configuration["Smtp:Port"];
            int port = int.TryParse(portStr, out int parsedPort) ? parsedPort : 587;
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"];
            var fromName = _configuration["Smtp:FromName"] ?? "AiNutritionTracking";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = "Yêu cầu đặt lại mật khẩu - AiNutritionTracking";

            message.Body = new TextPart("html")
            {
                Text = $@"
            <div style='font-family: Arial, sans-serif; max-width:600px; margin:0 auto;'>
                <h3>Xin chào {fullName},</h3>
                <p>Bạn hoặc ai đó đã yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                <p>Nhấn vào nút bên dưới để đặt lại mật khẩu (hiệu lực trong 15 phút):</p>
                <p style='text-align:center;'>
                    <a href='{resetLink}' style='display:inline-block;padding:10px 20px;background:#0078d4;color:#fff;border-radius:6px;text-decoration:none;'>Đặt lại mật khẩu</a>
                </p>
                <p>Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
                <br/>
                <p>Trân trọng,<br/>AiNutritionTracking</p>
            </div>"
            };

            using var client = new SmtpClient();
            client.Timeout = 10000; // Đặt giới hạn 10 giây

            try
            {
                using var cts = new CancellationTokenSource(10000);
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cts.Token);
                await client.AuthenticateAsync(username, password, cts.Token);
                await client.SendAsync(message, cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService Error]: Gửi mail Reset Password thất bại. Chi tiết: {ex.Message}");
                throw;
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}