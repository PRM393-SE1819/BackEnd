namespace AiNutritionTracking.API.DTOs.Weight
{
    public class WeightChartDto
    {
        public DateTime? Date { get; set; }

        public double? Weight { get; set; }

        public double? BodyFat { get; set; }
    }
}