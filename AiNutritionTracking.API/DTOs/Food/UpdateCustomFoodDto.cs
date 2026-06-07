namespace AiNutritionTracking.API.DTOs.Food
{
    public class UpdateCustomFoodDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Calories { get; set; }
        public double? Protein { get; set; }
        public double? Carbs { get; set; }
        public double? Fat { get; set; }
        public string? ServingSize { get; set; }
        public string? Barcode { get; set; }
    }
}