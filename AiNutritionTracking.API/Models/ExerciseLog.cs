using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class ExerciseLog
{
    public int ExerciseLogId { get; set; }

    public int? UserId { get; set; }

    public int? ExerciseId { get; set; }

    public int? DurationMinutes { get; set; }

    public double? CaloriesBurned { get; set; }

    public DateTime? LoggedAt { get; set; }

    public virtual Exercise? Exercise { get; set; }

    public virtual User? User { get; set; }
}
