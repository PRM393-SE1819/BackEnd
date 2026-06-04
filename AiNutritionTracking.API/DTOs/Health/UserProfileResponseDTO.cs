using System;

namespace AiNutritionTracking.API.DTOs.Health
{
    public class UserProfileResponseDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public string? ActivityLevel { get; set; }
        public string? Goal { get; set; }
        public double? TargetWeight { get; set; }
        public int? CaloriesTarget { get; set; }
        public double? ProteinTarget { get; set; }
        public double? CarbTarget { get; set; }
        public double? FatTarget { get; set; }
        public double? BMI { get; set; }
        public double? BodyFat { get; set; }
    }
}
