using System.Text.Json;

namespace MS.API.Mini.Data.Models;

public class MonitoringRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required, ForeignKey(nameof(ServiceId))] public Guid ServiceId { get; set; }
    
    [Required, StringLength(125)]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(250)]
    public string AlertMessage { get; set; } = string.Empty;
    
    [StringLength(75)]
    public string MetricName { get; set; } = string.Empty;
    public JsonDocument Conditions { get; set; } = null!;
    public List<string> AlertChannels { get; set; } = null!;
    public List<string> RecipientUserIds { get; set; } = [];
    public JsonDocument? Constraints { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastEvaluated { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    [NotMapped]
    public List<ActiveDirectoryUserMiniDTO> Recipients { get; set; } = [];
    
    [Required]
    public long CreatedBy { get; set; }
    
    [ForeignKey("CreatedBy")]
    public DBUser? Creator { get; set; }
    
    [NotMapped]
    public string CreatedByName => Creator?.FullName ?? "Unknown";
}

public class RuleQueryParameters
{
    public string? MetricName { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? Search { get; set; }
}

public class CreateRuleRequest
{
    [Required] [StringLength(255)] public string Name { get; set; } = string.Empty;
    
    [Required, ForeignKey(nameof(ServiceId))] public string ServiceId { get; set; }

    public string Description { get; set; } = string.Empty;

    [Required] public string MetricName { get; set; } = string.Empty;

    [Required] public RuleConditions Conditions { get; set; } = null!;

    [Required] public List<string> AlertChannels { get; set; } = [];
    
    public List<long> RecipientUserIds { get; set; } = [];

    public RuleConstraints? Constraints { get; set; }

    [Range(1, 4)] public int Priority { get; set; } = 1;
}

public class UpdateRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public int Priority { get; set; } = 1;

    public RuleConditions Conditions { get; set; } = null!;
    
    public List<long> RecipientUserIds { get; set; } = [];

    public List<string> AlertChannels { get; set; } = null!;
    
    public string MetricName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class RuleConditions
{
    [AllowedValues(">", "<", ">=", "<=", "==", "!=")]
    public string Operator { get; set; } = string.Empty;
    
    public decimal Threshold { get; set; }
    
    public int MaxAlertsPerHour { get; set; }
    
    public int EvaluationWindow { get; set; } // Minutes
    
    public int ConsecutiveBreaches { get; set; } = 3;
    
    [AllowedValues("avg", "max", "min", "sum")]
    public string? AggregationMethod { get; set; }
}

// DTOs/RuleConstraints.cs
public class RuleConstraints
{
    public List<string> ExcludeMetrics { get; set; } = new();
    public TimeRangeConstraint? TimeRange { get; set; }
    public int MaxSimilarRules { get; set; } = 5;
    public bool PreventDuplicateThresholds { get; set; } = true;
}

[Keyless]
public class RuleConflict
{
    public Guid RuleId1 { get; set; }
    public Guid RuleId2 { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ConflictType { get; set; } = string.Empty;
}

public class TimeRangeConstraint
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();
}