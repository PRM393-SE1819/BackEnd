using System;

namespace AiNutritionTracking.API.DTOs.Water
{
    public class WaterSummaryDto
    {
        public DateTime Date { get; set; }
        public float ConsumedML { get; set; }
        public float GoalML { get; set; }
        public float RemainingML { get; set; }
        public float Percentage { get; set; }
    }
}
