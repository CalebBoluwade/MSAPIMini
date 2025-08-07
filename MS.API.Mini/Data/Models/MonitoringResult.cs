namespace MS.API.Mini.Data.Models;

public record TrackerRecordDTO
{
    [JsonPropertyName("ServiceName")] public string ServiceName {  get; set; }
    [JsonPropertyName("IPAddress")] public string IPAddress {  get; set; }
    [JsonPropertyName("CurrentHealthCheck")] public MonitoringStatus CurrentHealthCheck {  get; set; }
}

// Main monitoring result entity
public class MonitoringResultHistory
{
    [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [JsonPropertyName("SystemMonitorId")]
    public Guid SystemMonitorId { get; set; }
        
    public DateTime ExecutionTime { get; set; } = DateTime.UtcNow;
        
    [Required]
    public MonitoringStatus Status { get; set; }

    [Required] public MonitoringStatus MainStatus { get; set; } = MonitoringStatus.UnknownStatus;
    
    /// <summary>
    /// The result of the plugin check.
    /// </summary>
    [MaxLength(200)]
    public string? HealthReport { get; set; }

    // Navigation properties
    public virtual SystemMonitor Service { get; set; } = null!;
    public virtual ICollection<PluginMonitoringResult> PluginMonitoringResults { get; set; } = new List<PluginMonitoringResult>();
}

/// <summary>
/// Represents a detailed, key-value result from a specific monitoring plugin.
/// This is the child entity in the relationship.
/// </summary>
[Table("PluginMonitoringResults")]
public class PluginMonitoringResult
{
    /// <summary>
    /// The unique identifier for the plugin result.
    /// </summary>
    [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
        
    /// <summary>
    /// Foreign key to the main MonitoringResult.
    /// This links the plugin data back to its parent monitoring check.
    /// </summary>
    [Required]
    public Guid MonitoringResultId { get; set; }
        
    [Required, NotMapped]
    public virtual PluginType PluginType { get; set; }
    
    /// <summary>
    /// The Name of the plugin that generated this result.
    /// </summary>
    [Required, JsonPropertyName("Name")]
    public string PluginName { get; set; }
    
    [Required, JsonPropertyName("Description")]
    public string PluginDescription { get; set; }
    
    [Required, JsonPropertyName("Metrics")]
    public string PluginMetrics { get; set; }
        
    [Required]
    public MonitoringStatus Status { get; set; }
        
    /// <summary>
    /// The result of the plugin check.
    /// </summary>
    [MaxLength(1000)]
    public string? HealthReport { get; set; }

    // Navigation properties
    /// <summary>
    /// Navigation property to the parent MonitoringResult.
    /// This establishes the "one" side of the relationship.
    /// </summary>
    [ForeignKey(nameof(MonitoringResultId))]
    public virtual MonitoringResultHistory MonitoringResult { get; set; } = null!;
}


public class MonitoringResultDTO
{
    public Guid Id { get; set; }
    
    public Guid SystemMonitorId { get; set; }

    public string Status { get; set; } = string.Empty;
    
    public string MainStatus { get; set; } = string.Empty;

    public DateTime CheckedAt { get; set; }

    public string? Message { get; set; }

    public List<PluginResultDTO> PluginResults { get; set; } = [];
}

public class PluginResultDTO
{
    public required string PluginName { get; set; } = string.Empty;
    public required string PluginDescription { get; set; } = string.Empty;
    
    [NotMapped]
    public PluginType PluginType { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Output { get; set; }
    
    public object? PluginMetrics { get; set; }
    public DateTime CheckedAt { get; set; }
}