using FluentValidation;

namespace MS.API.Mini.Data.Models.Validations;

public class CreateRuleRequestValidator : AbstractValidator<CreateRuleRequest>
{
    public CreateRuleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rule name is required")
            .MaximumLength(100).WithMessage("Rule name cannot exceed 100 characters");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required")
            .Must(id => id == "*" || Guid.TryParse(id, out _))
            .WithMessage("Invalid service ID");

        RuleFor(x => x.MetricName)
            .NotEmpty().WithMessage("Metric name is required")
            .MaximumLength(50).WithMessage("Metric name cannot exceed 50 characters");

        RuleFor(x => x.Conditions)
            .NotNull().WithMessage("Conditions are required")
            .Must(BeValidConditions).WithMessage("Invalid conditions format");

        RuleFor(x => x.Constraints)
            .SetValidator(new RuleConstraintsValidator()!);


    }

    private static bool BeValidConditions(RuleConditions? conditions)
    {
        return conditions is { Threshold: > 0 } &&
               !string.IsNullOrEmpty(conditions.Operator);
    }
}

public class RuleConstraintsValidator : AbstractValidator<RuleConstraints>
{
    public RuleConstraintsValidator()
    {
        RuleFor(x => x.MaxSimilarRules)
            .GreaterThanOrEqualTo(0).WithMessage("MaxSimilarRules must be non-negative");
    }
}