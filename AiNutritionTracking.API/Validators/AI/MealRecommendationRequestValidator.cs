using AiNutritionTracking.API.DTOs.AI;
using FluentValidation;

namespace AiNutritionTracking.API.Validators.AI;

public class MealRecommendationRequestValidator : AbstractValidator<MealRecommendationRequestDto>
{
    private static readonly string[] ValidActivityLevels = { "Sedentary", "Light", "Moderate", "Active", "Very Active" };
    private static readonly string[] ValidGenders = { "Male", "Female", "Other" };

    public MealRecommendationRequestValidator()
    {
        RuleFor(x => x.Goal)
            .NotEmpty().WithMessage("Goal is required.");

        RuleFor(x => x.Age)
            .InclusiveBetween(10, 120).WithMessage("Age must be between 10 and 120.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .Must(g => ValidGenders.Contains(g, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Gender must be Male, Female, or Other.");

        RuleFor(x => x.Weight)
            .InclusiveBetween(20, 500).WithMessage("Weight must be between 20 and 500 kg.");

        RuleFor(x => x.Height)
            .InclusiveBetween(50, 300).WithMessage("Height must be between 50 and 300 cm.");

        RuleFor(x => x.ActivityLevel)
            .NotEmpty().WithMessage("Activity level is required.")
            .Must(a => ValidActivityLevels.Contains(a, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Activity level must be one of: Sedentary, Light, Moderate, Active, Very Active.");
    }
}
