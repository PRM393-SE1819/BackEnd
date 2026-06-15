using AiNutritionTracking.API.DTOs.AI;
using FluentValidation;

namespace AiNutritionTracking.API.Validators.AI;

public class BodyFatImageRequestValidator : AbstractValidator<BodyFatImageRequestDto>
{
    private static readonly string[] ValidGenders = { "Male", "Female" };

    public BodyFatImageRequestValidator()
    {
        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .Must(g => ValidGenders.Contains(g, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Gender must be 'Male' or 'Female'.");

        RuleFor(x => x.Age)
            .InclusiveBetween(10, 100).WithMessage("Age must be between 10 and 100.");

        RuleFor(x => x.Height)
            .InclusiveBetween(100, 250).WithMessage("Height must be between 100 and 250 cm.");

        RuleFor(x => x.Weight)
            .InclusiveBetween(20, 300).WithMessage("Weight must be between 20 and 300 kg.");

        RuleFor(x => x.Images)
            .NotEmpty().WithMessage("At least one body image is required.")
            .Must(imgs => imgs.Count <= 3).WithMessage("Maximum 3 images allowed.");
    }
}

public class BodyFatMeasurementRequestValidator : AbstractValidator<BodyFatMeasurementRequestDto>
{
    private static readonly string[] ValidGenders = { "Male", "Female" };

    public BodyFatMeasurementRequestValidator()
    {
        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .Must(g => ValidGenders.Contains(g, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Gender must be 'Male' or 'Female'.");

        RuleFor(x => x.Age)
            .InclusiveBetween(10, 100).WithMessage("Age must be between 10 and 100.");

        RuleFor(x => x.Height)
            .InclusiveBetween(100, 250).WithMessage("Height must be between 100 and 250 cm.");

        RuleFor(x => x.Weight)
            .InclusiveBetween(20, 300).WithMessage("Weight must be between 20 and 300 kg.");

        RuleFor(x => x.Waist)
            .InclusiveBetween(40, 200).WithMessage("Waist must be between 40 and 200 cm.");

        RuleFor(x => x.Neck)
            .InclusiveBetween(20, 80).WithMessage("Neck must be between 20 and 80 cm.");

        RuleFor(x => x.Waist)
            .GreaterThan(x => x.Neck).WithMessage("Waist circumference must be greater than neck circumference.");

        RuleFor(x => x.Hip)
            .InclusiveBetween(40, 200).When(x => x.Hip.HasValue)
            .WithMessage("Hip must be between 40 and 200 cm.");

        RuleFor(x => x.Hip)
            .NotNull().When(x => string.Equals(x.Gender, "Female", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Hip measurement is required for the female US Navy formula.");
    }
}
