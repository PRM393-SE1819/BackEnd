using System;

namespace AiNutritionTracking.API.DTOs.Water
{
    public class WaterLogDto
    {
        public int WaterLogId { get; set; }
        public float AmountML { get; set; }
        public DateTime LoggedAt { get; set; }
    }
}
