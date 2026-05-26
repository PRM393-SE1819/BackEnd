using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class Recipe
{
    public int RecipeId { get; set; }

    public int? UserId { get; set; }

    public string? RecipeName { get; set; }

    public string? Description { get; set; }

    public string? Instructions { get; set; }

    public double? TotalCalories { get; set; }

    public int? CookingTime { get; set; }

    public string? ImageUrl { get; set; }

    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<RecipeItem> RecipeItems { get; set; } = new List<RecipeItem>();

    public virtual User? User { get; set; }
}
