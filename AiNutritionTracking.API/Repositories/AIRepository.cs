using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AiNutritionTracking.API.Repositories;

public class AIRepository : IAIRepository
{
    private readonly AinutritiontrackingContext _context;

    public AIRepository(AinutritiontrackingContext context)
    {
        _context = context;
    }

    public async Task<Airequest> SaveRequestAsync(int userId, string requestType, string prompt, string aiProvider, string aiModel)
    {
        var request = new Airequest
        {
            UserId = userId,
            RequestType = requestType,
            Prompt = prompt,
            Aiprovider = aiProvider,
            Aimodel = aiModel,
            Status = "Processing",
            RequestedAt = DateTime.UtcNow
        };

        _context.Airequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task SaveResponseAsync(int requestId, string responseContent, int tokensUsed, int responseTimeMs)
    {
        var request = await _context.Airequests.FindAsync(requestId);
        if (request != null)
        {
            request.Status = "Completed";
            request.TokensUsed = tokensUsed;
            request.ResponseTimeMs = responseTimeMs;
        }

        var response = new Airesponse
        {
            RequestId = requestId,
            RawResponse = responseContent,
            CreatedAt = DateTime.UtcNow
        };

        _context.Airesponses.Add(response);
        await _context.SaveChangesAsync();
    }

    public async Task MarkRequestFailedAsync(int requestId, string errorMessage)
    {
        var request = await _context.Airequests.FindAsync(requestId);
        if (request != null)
        {
            request.Status = "Failed";
            request.ErrorMessage = errorMessage;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Airequest>> GetUserRequestHistoryAsync(int userId, int page = 1, int pageSize = 20)
    {
        return await _context.Airequests
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Airequest>> GetChatHistoryAsync(int userId, int page = 1, int pageSize = 20)
    {
        return await _context.Airequests
            .Include(r => r.Airesponses)
            .Where(r => r.UserId == userId && r.RequestType == "Chat")
            .OrderByDescending(r => r.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> DeleteChatRecordAsync(int requestId, int userId)
    {
        var record = await _context.Airequests
            .Include(r => r.Airesponses)
            .FirstOrDefaultAsync(r => r.RequestId == requestId && r.UserId == userId && r.RequestType == "Chat");

        if (record is null) return false;

        _context.Airesponses.RemoveRange(record.Airesponses);
        _context.Airequests.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteAllChatHistoryAsync(int userId)
    {
        var records = await _context.Airequests
            .Include(r => r.Airesponses)
            .Where(r => r.UserId == userId && r.RequestType == "Chat")
            .ToListAsync();

        foreach (var record in records)
            _context.Airesponses.RemoveRange(record.Airesponses);

        _context.Airequests.RemoveRange(records);
        await _context.SaveChangesAsync();
        return records.Count;
    }
}
