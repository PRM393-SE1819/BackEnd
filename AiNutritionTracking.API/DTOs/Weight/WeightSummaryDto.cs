namespace AiNutritionTracking.API.DTOs.Weight
{
    public class WeightSummaryDto
    {
        public double? CurrentWeight { get; set; }

        public double? TargetWeight { get; set; }

        public double? WeightDifference { get; set; }

        public double? CurrentBodyFat { get; set; }
    }
}