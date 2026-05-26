using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class WaterLog
{
    public int WaterLogId { get; set; }

    public int? UserId { get; set; }

    public double? AmountMl { get; set; }

    public DateTime? LoggedAt { get; set; }

    public virtual User? User { get; set; }
}
