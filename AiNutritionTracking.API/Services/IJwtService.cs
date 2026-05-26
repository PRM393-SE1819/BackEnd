using AiNutritionTracking.API.Models;

namespace AiNutritionTracking.API.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
