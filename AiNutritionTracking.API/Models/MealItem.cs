using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class MealItem
{
    public int MealItemId { get; set; }

    public int? MealId { get; set; }

    public int? FoodId { get; set; }

    public double? Quantity { get; set; }

    public double? Calories { get; set; }

    public double? Protein { get; set; }

    public double? Carbs { get; set; }

    public double? Fat { get; set; }

    public virtual Food? Food { get; set; }

    public virtual Meal? Meal { get; set; }
}
