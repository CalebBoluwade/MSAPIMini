using System.Text.Json;

namespace MS.API.Mini.Data.Models;

public class MonitoringRule
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(50)] public string Name { get; set; } = string.Empty;

    [Required, ForeignKey(nameof(ServiceId))]
    public Guid ServiceId { get; set; }

    [Required, StringLength(125)] public string Description { get; set; } = string.Empty;

    [StringLength(250)] public string AlertMessage { get; set; } = string.Empty;

    [StringLength(75)] public string MetricName { get; set; } = string.Empty;
    public JsonDocument Conditions { get; set; } = null!;
    public List<string> AlertChannels { get; set; } = null!;
    public string[] RecipientUserIds { get; set; } = [];
    public JsonDocument? Constraints { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastEvaluated { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [NotMapped] public List<ActiveDirectoryUserMiniDTO> Recipients { get; set; } = [];

    [Required, JsonIgnore] public Guid CreatedBy { get; set; }

    [ForeignKey("CreatedBy"), JsonIgnore] public DBUser? Creator { get; set; }

    [NotMapped, JsonPropertyName("CreatedBy")]
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

    [Required, ForeignKey(nameof(ServiceId))]
    public string ServiceId { get; set; }

    public string Description { get; set; } = string.Empty;

    [Required] public string MetricName { get; set; } = string.Empty;

    [Required] public RuleConditions Conditions { get; set; } = null!;

    [Required] public List<string> AlertChannels { get; set; } = [];

    public List<Guid> RecipientUserIds { get; set; } = [];

    public RuleConstraints? Constraints { get; set; }

    [Range(1, 4)] public int Priority { get; set; } = 1;
}

public class UpdateRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int Priority { get; set; } = 1;

    public RuleConditions Conditions { get; set; } = null!;

    public List<Guid> RecipientUserIds { get; set; } = [];

    public List<string> AlertChannels { get; set; } = null!;

    public string MetricName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class RuleConditions
{
    [Required, AllowedValues(">", "<", ">=", "<=", "==", "!=")]
    public string Operator { get; set; } = string.Empty;

    [Required] public decimal Threshold { get; set; }

    [Required, Range(1, 24)] public int MaxAlertsPerHour { get; set; }

    [Required] public string AlertThrottleTime { get; set; } = "5m"; // e.g., "5m", "30s", "1h"

    [Required] public string EvaluationWindow { get; set; } = "5m"; // e.g., "5m", "10m", "1h"

    [Range(1, int.MaxValue)] public int ConsecutiveBreaches { get; set; } = 3;

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

public class RuleConflict
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RuleId1 { get; set; }
    public Guid RuleId2 { get; set; }
    
    [StringLength(100)]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(250)]
    public string ConflictType { get; set; } = string.Empty;
}

public class TimeRangeConstraint
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();
}

public class RuleEvaluation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RuleId { get; set; }

    [Required]
    public decimal MetricValue { get; set; }

    [Required]
    public bool ThresholdBreached { get; set; }

    [Required]
    public DateTime EvaluationTime { get; set; }

    [Required]
    public TimeSpan EvaluationDuration { get; set; }

    [Required]
    public bool AlertFired { get; set; }

    [ForeignKey(nameof(RuleId))]
    public virtual MonitoringRule Rule { get; set; } = null!;
}