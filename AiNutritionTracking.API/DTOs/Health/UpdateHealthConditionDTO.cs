using System.ComponentModel.DataAnnotations;

namespace AiNutritionTracking.API.DTOs.Health
{
    public class UpdateHealthConditionDTO
    {
        public string? ConditionName { get; set; } = null!;
        public string? Notes { get; set; }
    }
}
