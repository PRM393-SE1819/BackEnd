namespace AiNutritionTracking.API.DTOs.Meal
{
    public class MealItemDto
    {
        public int MealItemId { get; set; }
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public float Quantity { get; set; }
        public float Calories { get; set; }
        public float Protein { get; set; }
        public float Carbs { get; set; }
        public float Fat { get; set; }
    }
}