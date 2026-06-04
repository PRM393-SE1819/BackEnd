namespace AiNutritionTracking.API.DTOs.Health
{
    public class HealthConditionResponseDTO
    {
        public int ConditionId { get; set; }
        public string ConditionName { get; set; } = null!;
        public string? Notes { get; set; }
        public System.DateTime? CreatedAt { get; set; }
    }
}
