using FluentValidation;

namespace MS.API.Mini.Data.Models.Validations;

public class UpdateRuleRequestValidator : AbstractValidator<UpdateRuleRequest>
{
    private readonly MonitorDBContext _context;

    public UpdateRuleRequestValidator(MonitorDBContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rule name is required")
            .MaximumLength(100).WithMessage("Rule name cannot exceed 100 characters")
            .MustAsync(BeUniqueName).WithMessage("A rule with this name already exists");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Conditions)
            .NotNull().WithMessage("Conditions are required")
            .Must(BeValidConditions).WithMessage("Invalid conditions format");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5).WithMessage("Priority must be between 1 and 100");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        return !await _context.MonitoringRules
            .AnyAsync(r => r.Name == name, cancellationToken);
    }

    private bool BeValidConditions(RuleConditions? conditions)
    {
        return conditions != null && 
               conditions.Threshold > 0 &&
               !string.IsNullOrEmpty(conditions.Operator);
    }
}