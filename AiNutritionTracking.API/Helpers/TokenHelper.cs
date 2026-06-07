using System.Security.Cryptography;

namespace AiNutritionTracking.API.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateSecureToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
                .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        public static string HashToken(string token)
        {
            var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes).ToLower();
        }

        public static bool VerifyToken(string token, string hash)
        {
            return HashToken(token) == hash;
        }
    }
}