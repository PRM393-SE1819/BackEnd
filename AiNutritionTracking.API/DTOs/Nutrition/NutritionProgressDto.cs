namespace AiNutritionTracking.API.DTOs.Nutrition
{
    public class NutritionProgressDto
    {
        public string Nutrient { get; set; } = string.Empty;
        public float Consumed { get; set; }
        public float Target { get; set; }
        public float Remaining { get; set; }
        public float Percentage { get; set; }
    }
}