using MS.API.Mini.Models;

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
    public int RetryCount { get; set; }
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
    
    [JsonPropertyName("CurrentHealthCheck")]
    public MonitoringStatus CurrentHealthCheck { get; set; } = MonitoringStatus.UnknownStatus;

    [JsonPropertyName("CreatedAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<MonitoringResultHistory> MonitoringResults { get; set; } = new List<MonitoringResultHistory>();
}