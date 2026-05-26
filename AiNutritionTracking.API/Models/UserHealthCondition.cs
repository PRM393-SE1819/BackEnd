using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class UserHealthCondition
{
    public int ConditionId { get; set; }

    public int? UserId { get; set; }

    public string? ConditionName { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
