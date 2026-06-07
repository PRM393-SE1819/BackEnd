namespace AiNutritionTracking.API.DTOs.Food
{
    public class FoodDetailDto
    {
        public int FoodId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Calories { get; set; }
        public double? Protein { get; set; }
        public double? Carbs { get; set; }
        public double? Fat { get; set; }
        public double? Fiber { get; set; }
        public double? Sugar { get; set; }
        public double? Sodium { get; set; }
        public string? ServingSize { get; set; }
        public string? Barcode { get; set; }
        public string? ImageUrl { get; set; }
        public string? FoodType { get; set; }
        public bool? IsVerified { get; set; }
    }
}