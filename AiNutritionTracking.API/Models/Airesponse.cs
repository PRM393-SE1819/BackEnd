using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class Airesponse
{
    public int ResponseId { get; set; }

    public int? RequestId { get; set; }

    public string? RawResponse { get; set; }

    public string? Summary { get; set; }

    public double? ConfidenceScore { get; set; }

    public double? CaloriesEstimate { get; set; }

    public double? ProteinEstimate { get; set; }

    public double? CarbEstimate { get; set; }

    public double? FatEstimate { get; set; }

    public string? GeneratedMealPlan { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AiextractedField> AiextractedFields { get; set; } = new List<AiextractedField>();

    public virtual Airequest? Request { get; set; }
}
