using System.Collections.Generic;
using System.Threading.Tasks;
using AiNutritionTracking.API.DTOs.Health;

namespace AiNutritionTracking.API.Services
{
    public interface IHealthProfileService
    {
        Task<UserProfileResponseDTO?> GetUserProfileAsync(int userId);
        Task<(bool Success, string Message, UserProfileResponseDTO? Data)> UpdateUserProfileAsync(int userId, UpdateUserProfileDTO request);
        
        Task<List<HealthConditionResponseDTO>> GetHealthConditionsAsync(int userId);
        Task<(bool Success, string Message, HealthConditionResponseDTO? Data)> AddHealthConditionAsync(int userId, AddHealthConditionDTO request);
        Task<(bool Success, string Message)> UpdateHealthConditionAsync(int userId, int conditionId, UpdateHealthConditionDTO request);
        Task<(bool Success, string Message)> DeleteHealthConditionAsync(int userId, int conditionId);

        Task<List<AllergyResponseDTO>> GetAllergiesAsync(int userId);
        Task<(bool Success, string Message, AllergyResponseDTO? Data)> AddAllergyAsync(int userId, AddAllergyDTO request);
        Task<(bool Success, string Message)> UpdateAllergyAsync(int userId, int allergyId, AddAllergyDTO request);
        Task<(bool Success, string Message)> DeleteAllergyAsync(int userId, int allergyId);
    }
}
