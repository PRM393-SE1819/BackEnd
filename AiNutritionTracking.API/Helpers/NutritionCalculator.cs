using System;

namespace AiNutritionTracking.API.Helpers
{
    public static class NutritionCalculator
    {
        // 1. Tính số tuổi 
        public static int CalculateAge(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth > today.AddYears(-age)) age--;
            return age;
        }

        // 2. Tính TDEE sử dụng phương trình Mifflin-St Jeor
        public static double CalculateTDEE(string gender, double weightKg, double heightCm, int age, string activityLevel)
        {
            // Tính BMR (Basal Metabolic Rate)
            double bmr = (10 * weightKg) + (6.25 * heightCm) - (5 * age);
            
            if (gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
                bmr += 5;
            else if (gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
                bmr -= 161;

            // Xác định hệ số hoạt động 
            double multiplier = activityLevel switch
            {
                "Sedentary" => 1.2,
                "LightlyActive" => 1.375,
                "ModeratelyActive" => 1.55,
                "VeryActive" => 1.725,
                "ExtraActive" => 1.9,
                _ => 1.2
            };

            return bmr * multiplier;
        }

        // 3. Tính toán mục tiêu Calories & Macros
        public static (int Calories, double Protein, double Carbs, double Fat) CalculateTargets(double tdee, string goal)
        {
            // Điều chỉnh Calories theo Goal
            int targetCalories = goal switch
            {
                "LoseWeight" => (int)(tdee - 500),
                "GainWeight" => (int)(tdee + 500),
                _ => (int)tdee // Mặc định là MaintainWeight
            };

            // Mốc tối thiểu an toàn để tham khảo là 1200
            if (targetCalories < 1200) targetCalories = 1200;

            // Tính Macros: Protein (30%), Carbs (40%), Fat (30%)
            // 1g Protein = 4 kcal, 1g Carb = 4 kcal, 1g Fat = 9 kcal
            double proteinGram = (targetCalories * 0.3) / 4.0;
            double carbGram = (targetCalories * 0.4) / 4.0;
            double fatGram = (targetCalories * 0.3) / 9.0;

            return (targetCalories, Math.Round(proteinGram, 1), Math.Round(carbGram, 1), Math.Round(fatGram, 1));
        }

        // 4. Tính BMI
        public static double CalculateBMI(double weightKg, double heightCm)
        {
            if (heightCm <= 0) return 0;
            double heightM = heightCm / 100.0;
            return Math.Round(weightKg / (heightM * heightM), 1);
        }

        // 5. Tính Tỉ lệ Body Fat ước lượng (Deurenberg et al.)
        public static double CalculateBodyFat(double bmi, int age, string gender)
        {
            int sexValue = gender.Equals("Male", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            double bodyFat = (1.20 * bmi) + (0.23 * age) - (10.8 * sexValue) - 5.4;
            return Math.Round(Math.Max(0, bodyFat), 1); // Đảm bảo không bị âm
        }
    }
}
