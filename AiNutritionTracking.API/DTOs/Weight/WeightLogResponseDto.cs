namespace AiNutritionTracking.API.DTOs.Weight
{
    public class WeightLogResponseDto
    {
        public int WeightLogId { get; set; }

        public double? Weight { get; set; }

        public double? BodyFat { get; set; }

        public DateTime? LoggedAt { get; set; }
    }
}