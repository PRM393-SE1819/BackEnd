using AiNutritionTracking.API.DTOs.AI;
using FluentValidation;

namespace AiNutritionTracking.API.Validators.AI;

public class ChatRequestValidator : AbstractValidator<ChatRequestDto>
{
    public ChatRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message cannot be empty.")
            .MinimumLength(3).WithMessage("Message must be at least 3 characters.")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters.");
    }
}
