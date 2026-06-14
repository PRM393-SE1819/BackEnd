using AiNutritionTracking.API.DTOs.AI;
using FluentValidation;

namespace AiNutritionTracking.API.Validators.AI;

public class MealPlanRequestValidator : AbstractValidator<MealPlanRequestDto>
{
    public MealPlanRequestValidator()
    {
        RuleFor(x => x.Goal)
            .NotEmpty().WithMessage("Goal is required.")
            .MaximumLength(100).WithMessage("Goal must not exceed 100 characters.");

        RuleFor(x => x.DailyCalories)
            .InclusiveBetween(800, 10000).WithMessage("Daily calories must be between 800 and 10,000 kcal.");
    }
}
