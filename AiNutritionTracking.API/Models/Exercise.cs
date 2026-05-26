using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class Exercise
{
    public int ExerciseId { get; set; }

    public string? ExerciseName { get; set; }

    public double? CaloriesBurnedPerHour { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ExerciseLog> ExerciseLogs { get; set; } = new List<ExerciseLog>();
}
