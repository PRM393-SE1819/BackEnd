using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class Airequest
{
    public int RequestId { get; set; }

    public int? UserId { get; set; }

    public string? RequestType { get; set; }

    public string? Prompt { get; set; }

    public string? InputImageUrl { get; set; }

    public string? Aiprovider { get; set; }

    public string? Aimodel { get; set; }

    public int? TokensUsed { get; set; }

    public int? ResponseTimeMs { get; set; }

    public string? Status { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? RequestedAt { get; set; }

    public virtual ICollection<Airesponse> Airesponses { get; set; } = new List<Airesponse>();

    public virtual User? User { get; set; }
}
