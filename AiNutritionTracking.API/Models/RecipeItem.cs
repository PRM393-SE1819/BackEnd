using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class RecipeItem
{
    public int RecipeItemId { get; set; }

    public int? RecipeId { get; set; }

    public int? FoodId { get; set; }

    public double? Quantity { get; set; }

    public string? Unit { get; set; }

    public virtual Food? Food { get; set; }

    public virtual Recipe? Recipe { get; set; }
}
