﻿using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
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
            var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
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
            try
            {
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService Error]: Gửi mail thất bại. Chi tiết: {ex.Message}");
                // Only rethrow in Production
                #if !DEBUG
                throw;
                #endif
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendEmailVerificationAsync(string toEmail, string fullName, string verificationUrl)
        {
            var body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px;'>
                        <h2>Xin chào {fullName},</h2>
                        <p>Vui lòng click vào link bên dưới để xác thực tài khoản:</p>
                        <a href='{verificationUrl}' style='background:#0078d4;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>Xác thực tài khoản</a>
                        <p>Link có hiệu lực trong 24 giờ.</p>
                      </div>";
            await SendMailAsync(toEmail, fullName, "Xác thực tài khoản", body);
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