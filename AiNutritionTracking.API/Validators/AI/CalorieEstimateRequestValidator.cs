using AiNutritionTracking.API.DTOs.AI;
using FluentValidation;

namespace AiNutritionTracking.API.Validators.AI;

public class CalorieEstimateRequestValidator : AbstractValidator<CalorieEstimateRequestDto>
{
    public CalorieEstimateRequestValidator()
    {
        RuleFor(x => x.FoodDescription)
            .NotEmpty().WithMessage("Food description cannot be empty.")
            .MinimumLength(3).WithMessage("Food description must be at least 3 characters.")
            .MaximumLength(1000).WithMessage("Food description must not exceed 1000 characters.");
    }
}
