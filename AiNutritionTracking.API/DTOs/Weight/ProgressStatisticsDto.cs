namespace AiNutritionTracking.API.DTOs.Weight
{
    public class ProgressStatisticsDto
    {
        public double? StartWeight { get; set; }

        public double? CurrentWeight { get; set; }

        public double? WeightChanged { get; set; }

        public double? StartBodyFat { get; set; }

        public double? CurrentBodyFat { get; set; }

        public double? BodyFatChanged { get; set; }

        public List<WeightChartDto> History { get; set; } = new();
    }
}