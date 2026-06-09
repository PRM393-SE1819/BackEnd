using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Auth;
using AiNutritionTracking.API.Helpers;
using AiNutritionTracking.API.Models;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AiNutritionTracking.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AinutritiontrackingContext _context;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _memoryCache;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthService(AinutritiontrackingContext context, IEmailService emailService, IMemoryCache memoryCache, IJwtService jwtService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _memoryCache = memoryCache;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                if (existingUser.EmailVerified == false)
                {
                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    existingUser.FullName = request.FullName;
                    existingUser.Username = request.Username;
                    existingUser.UpdatedAt = DateTime.UtcNow;

                    var newToken = TokenHelper.GenerateSecureToken();
                    existingUser.EmailVerificationTokenHash = TokenHelper.HashToken(newToken);
                    existingUser.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);

                    _context.Users.Update(existingUser);
                    await _context.SaveChangesAsync();

                    var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:8080";
                    var verificationUrl = $"{frontendUrl.TrimEnd('/')}/verify-email?email={Uri.EscapeDataString(existingUser.Email)}&token={Uri.EscapeDataString(newToken)}";
                    await _emailService.SendEmailVerificationAsync(existingUser.Email, existingUser.FullName, verificationUrl);

                    return new AuthResponseDTO { Success = true, Message = "Vui lòng kiểm tra email để xác thực tài khoản." };
                }
                return new AuthResponseDTO { Success = false, Message = "Email đã được sử dụng." };
            }

            var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUsername != null)
                return new AuthResponseDTO { Success = false, Message = "Username đã tồn tại." };

            var token = TokenHelper.GenerateSecureToken();

            var newUser = new User
            {
                Email = request.Email,
                Username = request.Username,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                EmailVerified = false,
                EmailVerificationTokenHash = TokenHelper.HashToken(token),
                EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var baseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:8080";
            var url = $"{baseUrl.TrimEnd('/')}/verify-email?email={Uri.EscapeDataString(newUser.Email)}&token={Uri.EscapeDataString(token)}";
            await _emailService.SendEmailVerificationAsync(newUser.Email, newUser.FullName, url);

            return new AuthResponseDTO { Success = true, Message = "Vui lòng kiểm tra email để xác thực tài khoản." };
        }

        public async Task<AuthResponseDTO> VerifyEmailAsync(VerifyEmailRequestDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return new AuthResponseDTO { Success = false, Message = "Không tìm thấy người dùng." };

            if (user.EmailVerified == true)
                return new AuthResponseDTO { Success = true, Message = "Tài khoản đã được xác thực." };

            if (string.IsNullOrWhiteSpace(user.EmailVerificationTokenHash))
                return new AuthResponseDTO { Success = false, Message = "Token xác thực không hợp lệ." };

            if (user.EmailVerificationTokenExpiresAt == null || user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
                return new AuthResponseDTO { Success = false, Message = "Token xác thực đã hết hạn." };

            if (!TokenHelper.VerifyToken(request.Token, user.EmailVerificationTokenHash))
                return new AuthResponseDTO { Success = false, Message = "Token xác thực không hợp lệ." };

            user.EmailVerified = true;
            user.EmailVerificationTokenHash = null;
            user.EmailVerificationTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AuthResponseDTO { Success = true, Message = "Xác thực Email thành công! Giờ bạn có thể đăng nhập." };
        }

        public async Task<AuthResponseDTO> ResendVerificationEmailAsync(ResendVerificationEmailRequestDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return new AuthResponseDTO { Success = false, Message = "Email chưa được đăng ký trong hệ thống." };

            if (user.EmailVerified == true)
                return new AuthResponseDTO { Success = false, Message = "Tài khoản này đã được xác thực rồi." };

            var token = TokenHelper.GenerateSecureToken();
            user.EmailVerificationTokenHash = TokenHelper.HashToken(token);
            user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var baseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:8080";
            var url = $"{baseUrl.TrimEnd('/')}/verify-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
            await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, url);

            return new AuthResponseDTO { Success = true, Message = "Vui lòng kiểm tra email để xác thực tài khoản." };
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.IsDeleted == true)
                return new LoginResponseDTO { Success = false, Message = "Email hoặc mật khẩu không đúng." };

            if (user.EmailVerified != true)
                return new LoginResponseDTO { Success = false, Message = "Vui lòng xác thực email trước khi đăng nhập." };

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return new LoginResponseDTO { Success = false, Message = "Email hoặc mật khẩu không đúng." };

            var token = _jwtService.GenerateToken(user);
            return new LoginResponseDTO { Success = true, Message = "Đăng nhập thành công.", Token = token };
        }

        public async Task<GoogleLoginResponseDTO> GoogleLoginAsync(GoogleLoginRequestDTO request)
        {
            var clientId = _configuration["Google:ClientId"];
            if (string.IsNullOrEmpty(clientId))
                throw new InvalidOperationException("Đăng nhập bằng Google hiện không khả dụng.");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings() { Audience = new[] { clientId } };
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch (InvalidJwtException)
            {
                throw new InvalidOperationException("Thông tin đăng nhập từ Google không hợp lệ.");
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Không thể xác thực thông tin đăng nhập từ Google.");
            }

            if (payload == null || string.IsNullOrEmpty(payload.Email))
                throw new InvalidOperationException("Không tìm thấy email từ Google.");

            if (payload.EmailVerified != true)
                throw new InvalidOperationException("Email Google chưa được xác minh.");

            var email = payload.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    Username = email.Split('@')[0],
                    FullName = payload.Name ?? email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    EmailVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    AvatarUrl = payload.Picture
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else if (user.EmailVerified != true)
            {
                user.EmailVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            var accessToken = _jwtService.GenerateToken(user);
            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");
            return new GoogleLoginResponseDTO { AccessToken = accessToken, ExpiresInMinutes = expireMinutes };
        }

        public async Task RevokeTokenAsync(string jti, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(jti)) return;
            var ttl = expiresAt.ToUniversalTime() - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero) return;
            _memoryCache.Set($"revoked:{jti}", true, ttl);
            await Task.CompletedTask;
        }

        public async Task<AuthResponseDTO> RequestPasswordResetAsync(RequestPasswordResetDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return new AuthResponseDTO { Success = true, Message = "Nếu email tồn tại, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu." };

            var cooldownKey = $"pwreset-cooldown:{user.Email}";
            if (_memoryCache.TryGetValue(cooldownKey, out _))
                return new AuthResponseDTO { Success = false, Message = "Bạn vừa gửi yêu cầu. Vui lòng thử lại sau vài phút." };

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
                        .Replace("+", "-").Replace("/", "_").TrimEnd('=');

            var tokenKey = $"pwreset:{token}";
            var userTokenTrackingKey = $"pwreset-active-token:{user.Email}";

            if (_memoryCache.TryGetValue(userTokenTrackingKey, out string? oldToken))
                _memoryCache.Remove($"pwreset:{oldToken}");

            _memoryCache.Set(tokenKey, user.Email, TimeSpan.FromMinutes(15));
            _memoryCache.Set(userTokenTrackingKey, token, TimeSpan.FromMinutes(15));
            _memoryCache.Set(cooldownKey, true, TimeSpan.FromSeconds(60));

            var frontendUrl = _configuration["Frontend:PasswordResetUrl"] ?? _configuration["Frontend:BaseUrl"];
            var resetLink = $"{frontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";
            _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);

            return new AuthResponseDTO { Success = true, Message = "Nếu email tồn tại, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu trong email." };
        }

        public async Task<AuthResponseDTO> ResetPasswordAsync(ResetPasswordDTO request)
        {
            var tokenKey = $"pwreset:{request.Token}";
            if (!_memoryCache.TryGetValue(tokenKey, out string? email))
                return new AuthResponseDTO { Success = false, Message = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn." };

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return new AuthResponseDTO { Success = false, Message = "Người dùng không tồn tại." };

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                return new AuthResponseDTO { Success = false, Message = "Mật khẩu mới không hợp lệ (tối thiểu 6 ký tự)." };

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _memoryCache.Remove(tokenKey);
            _memoryCache.Remove($"pwreset-active-token:{email}");

            return new AuthResponseDTO { Success = true, Message = "Đổi mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới." };
        }
    }
}