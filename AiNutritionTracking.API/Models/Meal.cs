using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class Meal
{
    public int MealId { get; set; }

    public int? UserId { get; set; }

    public string? MealType { get; set; }

    public DateOnly? MealDate { get; set; }

    public string? Notes { get; set; }

    public double? TotalCalories { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<MealItem> MealItems { get; set; } = new List<MealItem>();

    public virtual User? User { get; set; }
}
