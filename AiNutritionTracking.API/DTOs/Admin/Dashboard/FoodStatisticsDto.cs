namespace AiNutritionTracking.API.DTOs.Admin.Dashboard
{
    public class FoodStatisticsDto
    {
        public int TotalFoods { get; set; }

        public int VerifiedFoods { get; set; }

        public int UnverifiedFoods { get; set; }

        public int DeletedFoods { get; set; }
    }
}
