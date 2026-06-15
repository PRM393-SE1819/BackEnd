using AiNutritionTracking.API.Models;

namespace AiNutritionTracking.API.Repositories;

public interface IAIRepository
{
    Task<Airequest> SaveRequestAsync(int userId, string requestType, string prompt, string aiProvider, string aiModel);
    Task SaveResponseAsync(int requestId, string responseContent, int tokensUsed, int responseTimeMs);
    Task MarkRequestFailedAsync(int requestId, string errorMessage);
    Task<List<Airequest>> GetUserRequestHistoryAsync(int userId, int page = 1, int pageSize = 20);
    Task<List<Airequest>> GetChatHistoryAsync(int userId, int page = 1, int pageSize = 20);
    Task<bool> DeleteChatRecordAsync(int requestId, int userId);
    Task<int> DeleteAllChatHistoryAsync(int userId);
}
