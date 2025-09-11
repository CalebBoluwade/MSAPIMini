

namespace MS.API.Mini.Data.Models;

public abstract class SystemMonitorBase
{
    [Key, JsonPropertyName("SystemMonitorId")]
    public Guid SystemMonitorId { get; set; } = Guid.NewGuid();

    [JsonPropertyName("IPAddress")] public string IPAddress { get; set; } = string.Empty;
    
    [JsonPropertyName("Port")] public int Port { get; set; } = 0;

    [JsonPropertyName("IsMonitored")] public bool IsMonitored { get; set; } = true;
    
    [JsonPropertyName("Device")]
    public string Device { get; set; } = string.Empty;
    public int FailureCount { get; set; }
    
    [JsonPropertyName("Configuration")]
    public string Configuration { get; set; } = "{}";
    public string CheckInterval { get; set; } = "*/15 * * * *";
    public bool IsAcknowledged { get; set; }
    public DateTime? SnoozeUntil { get; set; }
}

[Table("MonitorPlugins")]
public class MonitorPlugin
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), JsonIgnore]
    public int Id { get; set; }
    
    [JsonPropertyName("Id"), StringLength(32)]
    public string PluginId { get; set; } = string.Empty;

    [JsonPropertyName("Name"), StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Description"), StringLength(150)]
    public string Description { get; set; } = string.Empty;
    
    public PluginType PluginType { get; set; }

    public List<string> CompatibleDeviceTypes { get; set; } = [];

    [JsonPropertyName("ComingSoon")] public bool ComingSoon { get; set; }
}

[Table("SystemMonitor")]
public class SystemMonitor : SystemMonitorBase
{
    [Key, JsonPropertyName("SystemMonitorId")]
    public new Guid SystemMonitorId { get; set; }

    [StringLength(100), JsonPropertyName("ServiceName")]
    public string ServiceName { get; set; } = string.Empty;

    [Required, JsonPropertyName("Description"), MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, JsonPropertyName("Plugins")]
    public List<string> Plugins { get; set; } = [];
    
    [NotMapped, JsonPropertyName("PluginDetails")]
    public List<MonitorPlugin> PluginDetails { get; set; } = [];

    [JsonPropertyName("LastCheckTime")] public DateTime LastCheckTime { get; set; }

    [JsonPropertyName("LastServiceUptime")]
    public DateTime LastServiceUpTime { get; set; }
    
    [JsonPropertyName("PollerNode")] public Guid PollerNode { get; set; }
    
    [JsonPropertyName("Agent")] public string Agent { get; set; } = string.Empty;
    
    [JsonPropertyName("CurrentHealthCheck")]
    public MonitoringStatus? CurrentHealthCheck { get; set; } = MonitoringStatus.UnknownStatus;

    [JsonPropertyName("CreatedAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<MonitoringResultHistory> MonitoringResults { get; set; } = new List<MonitoringResultHistory>();
    
    public virtual ICollection<SystemMetric> MonitorMetrics { get; set; } = new List<SystemMetric>();
}

public record OverviewMetric
{
    public long? Timestamp { get; set; }
    public double Metric { get; set; }
}

public class SystemMonitorDTO
{
    [Key, JsonPropertyName("SystemMonitorId")]
    public new Guid SystemMonitorId { get; set; }
    
    [JsonPropertyName("IPAddress")] public string IPAddress { get; set; } = string.Empty;

    [JsonPropertyName("ServiceName")]
    public string ServiceName { get; set; } = string.Empty;

    [Required, JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;

    [Required, JsonPropertyName("Plugins")]
    public List<string> Plugins { get; set; } = [];
    
    [NotMapped, JsonPropertyName("PluginDetails")]
    public List<MonitorPlugin> PluginDetails { get; set; } = [];
    
    [JsonPropertyName("Device")] public string Device { get; set; } = string.Empty;
    
    [JsonPropertyName("CurrentHealthCheck")] public MonitoringStatus? CurrentHealthCheck { get; set; } = MonitoringStatus.UnknownStatus;
    
    [JsonPropertyName("Configuration")]
    public string Configuration { get; set; } = "{}";
    public string CheckInterval { get; set; } = "*/15 * * * *";
    
    [JsonPropertyName("IsAcknowledged")] public bool IsAcknowledged { get; set; }
    
    [JsonPropertyName("IsMonitored")] public bool IsMonitored { get; set; } = true;
    
    [JsonPropertyName("CreatedAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("Metrics")]
    public ICollection<OverviewMetric> MonitorMetrics { get; set; } = new List<OverviewMetric>();
}

public class AvailablePoller
{
    [Key, JsonPropertyName("PollerId")] public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("IPAddress")] public string IPAddress { get; set; } = string.Empty;
}