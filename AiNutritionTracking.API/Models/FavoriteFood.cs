using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class FavoriteFood
{
    public int FavoriteFoodId { get; set; }

    public int? UserId { get; set; }

    public int? FoodId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Food? Food { get; set; }

    public virtual User? User { get; set; }
}
