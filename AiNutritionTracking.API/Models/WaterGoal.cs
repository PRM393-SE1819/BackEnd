using System;

namespace AiNutritionTracking.API.Models;

public partial class WaterGoal
{
    public int GoalId { get; set; }
    public int? UserId { get; set; }
    public double DailyTargetMl { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User? User { get; set; }
}