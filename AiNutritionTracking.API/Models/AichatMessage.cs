using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class AichatMessage
{
    public int MessageId { get; set; }

    public int? SessionId { get; set; }

    public string? SenderType { get; set; }

    public string? Message { get; set; }

    public int? TokensUsed { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AichatSession? Session { get; set; }
}
