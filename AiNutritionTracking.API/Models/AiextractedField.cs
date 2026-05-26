using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class AiextractedField
{
    public int FieldId { get; set; }

    public int? ResponseId { get; set; }

    public string? FieldType { get; set; }

    public string? RawValue { get; set; }

    public string? ParsedValue { get; set; }

    public double? Confidence { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Airesponse? Response { get; set; }
}
