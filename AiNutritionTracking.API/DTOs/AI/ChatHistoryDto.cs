namespace AiNutritionTracking.API.DTOs.AI;

public class ChatHistoryDto
{
    public int RequestId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int? TokensUsed { get; set; }
    public int? ResponseTimeMs { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? RequestedAt { get; set; }
}
