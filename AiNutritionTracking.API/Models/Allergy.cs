using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class Allergy
{
    public int AllergyId { get; set; }

    public int? UserId { get; set; }

    public string? AllergyName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
