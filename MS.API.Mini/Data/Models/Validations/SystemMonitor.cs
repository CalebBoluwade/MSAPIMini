using System.Text.Json;
using Cronos;
using FluentValidation;

namespace MS.API.Mini.Data.Models.Validations;

public class SystemMonitorDTOValidator : AbstractValidator<SystemMonitor>
{
    public SystemMonitorDTOValidator()
    {
        RuleFor(x => x.ServiceName).NotEmpty().WithMessage("Service name is required");
        // RuleFor(x => x.IPAddress).NotEmpty().Matches(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$");
        RuleFor(x => x.Port).GreaterThan(0);
        RuleFor(x => x.Configuration)
            .Must(BeValidJson)
            .WithMessage("Configuration must be valid JSON");
        RuleFor(x => x.CheckInterval)
            .NotEmpty()
            .Must(BeValidCron)
            .WithMessage("CheckInterval must be a valid cron expression");
    }
    
    private static bool BeValidCron(string cron)
    {
        try
        {
            CronExpression.Parse(cron, CronFormat.Standard);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool BeValidJson(string config)
    {
        if (string.IsNullOrWhiteSpace(config)) return false;
        try
        {
            JsonDocument.Parse(config);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
