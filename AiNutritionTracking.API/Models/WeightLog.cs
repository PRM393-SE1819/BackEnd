using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class WeightLog
{
    public int WeightLogId { get; set; }

    public int? UserId { get; set; }

    public double? Weight { get; set; }

    public double? BodyFat { get; set; }

    public DateTime? LoggedAt { get; set; }

    public virtual User? User { get; set; }
}
