using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class Airecommendation
{
    public int RecommendationId { get; set; }

    public int? UserId { get; set; }

    public string? RecommendationType { get; set; }

    public string? Content { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
