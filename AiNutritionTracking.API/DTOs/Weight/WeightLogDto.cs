namespace AiNutritionTracking.API.DTOs.Weight
{
    public class WeightLogDto
    {
        public int WeightLogId { get; set; }

        public float Weight { get; set; }

        public float? BodyFat { get; set; }

        public DateTime LoggedAt { get; set; }
    }
}
