namespace AiNutritionTracking.API.DTOs.Health
{
    public class AllergyResponseDTO
    {
        public int AllergyId { get; set; }
        public string AllergyName { get; set; } = null!;
        public System.DateTime? CreatedAt { get; set; }
    }
}
