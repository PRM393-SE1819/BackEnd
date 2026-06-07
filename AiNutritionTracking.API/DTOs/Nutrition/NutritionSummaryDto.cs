using System;

namespace AiNutritionTracking.API.DTOs.Nutrition
{
    public class NutritionSummaryDto
    {
        public DateTime Date { get; set; }

        public float CaloriesConsumed { get; set; }
        public float CaloriesTarget { get; set; }

        public float ProteinConsumed { get; set; }
        public float ProteinTarget { get; set; }

        public float CarbConsumed { get; set; }
        public float CarbTarget { get; set; }

        public float FatConsumed { get; set; }
        public float FatTarget { get; set; }
    }
}