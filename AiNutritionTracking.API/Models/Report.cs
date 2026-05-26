using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public int? ReporterUserId { get; set; }

    public string? TargetType { get; set; }

    public int? TargetId { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? ReporterUser { get; set; }
}
