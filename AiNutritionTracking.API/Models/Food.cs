using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class Food
{
    public int FoodId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double Calories { get; set; }

    public double? Protein { get; set; }

    public double? Carbs { get; set; }

    public double? Fat { get; set; }

    public double? Fiber { get; set; }

    public double? Sugar { get; set; }

    public double? Sodium { get; set; }

    public string? ServingSize { get; set; }

    public string? Barcode { get; set; }

    public string? ImageUrl { get; set; }

    public string? FoodType { get; set; }

    public bool? IsVerified { get; set; }

    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<FavoriteFood> FavoriteFoods { get; set; } = new List<FavoriteFood>();

    public virtual ICollection<MealItem> MealItems { get; set; } = new List<MealItem>();

    public virtual ICollection<RecipeItem> RecipeItems { get; set; } = new List<RecipeItem>();

    public virtual User? UpdatedByNavigation { get; set; }
}
