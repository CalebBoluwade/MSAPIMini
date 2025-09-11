using System.Text.Json;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Data.Models.Validations;

namespace MS.API.Mini.Services;

public class ValidationResult(bool isValid, List<string> errors = null)
{
    public bool IsValid { get; set; } = isValid;
    public List<string> Errors { get; set; } = errors ?? [];

    public static ValidationResult Valid() => new(true);
    public static ValidationResult Invalid(List<string> errors) => new(false, errors);

    // Implicit conversion to bool for easier checking
    public static implicit operator bool(ValidationResult result) => result.IsValid;
}

public interface IRuleValidationService
{
    Task<ValidationResult> ValidateRuleAsync(CreateRuleRequest request, Guid? existingRuleId = null);
    Task<ValidationResult> ValidateRuleAsync(UpdateRuleRequest request, Guid ruleId);
    Task<List<RuleConflict>> DetectConflictsAsync(MonitoringRule rule);
}

public class RuleValidationService(
    MonitorDBContext dbCtx)
    : IRuleValidationService
{
    public async Task<ValidationResult> ValidateRuleAsync(CreateRuleRequest request, Guid? existingRuleId = null)
    {
        var validator = new CreateRuleRequestValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.IsValid)
        {
            return ValidationResult.Valid();
        }

        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        return ValidationResult.Invalid(errors);
    }

    public async Task<ValidationResult> ValidateRuleAsync(UpdateRuleRequest request, Guid ruleId)
    {
        var validator = new UpdateRuleRequestValidator(dbCtx);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.IsValid)
        {
            return ValidationResult.Valid();
        }

        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        return ValidationResult.Invalid(errors);
    }

    public async Task<List<RuleConflict>> DetectConflictsAsync(MonitoringRule rule)
    {
        var conflicts = new List<RuleConflict>();
        var conditions = rule.Conditions.Deserialize<RuleConditions>();

        if (conditions == null) return conflicts;

        var otherRules = await dbCtx.MonitoringRules
            .Where(r => r.MetricName == rule.MetricName &&
                        r.IsActive &&
                        r.Id != rule.Id)
            .ToListAsync();

        foreach (var otherRule in otherRules)
        {
            var otherConditions = otherRule.Conditions.Deserialize<RuleConditions>();
            if (otherConditions == null) continue;

            // Check for contradictory thresholds
            if (conditions.Operator == ">" && otherConditions.Operator == "<" &&
                conditions.Threshold >= otherConditions.Threshold)
            {
                conflicts.Add(new RuleConflict
                {
                    RuleId1 = rule.Id,
                    RuleId2 = otherRule.Id,
                    ConflictType = "Contradictory Thresholds",
                    Description = $"Rule '{rule.Name}' threshold > {conditions.Threshold} " +
                                  $"conflicts with '{otherRule.Name}' threshold < {otherConditions.Threshold}"
                });
            }

            // Check for overlapping conditions
            if (conditions.Operator == otherConditions.Operator &&
                Math.Abs(conditions.Threshold - otherConditions.Threshold) / conditions.Threshold < 0.1m)
            {
                conflicts.Add(new RuleConflict
                {
                    RuleId1 = rule.Id,
                    RuleId2 = otherRule.Id,
                    ConflictType = "Overlapping Conditions",
                    Description = $"Rule '{rule.Name}' has similar threshold to '{otherRule.Name}'"
                });
            }
        }

        return conflicts;
    }
}