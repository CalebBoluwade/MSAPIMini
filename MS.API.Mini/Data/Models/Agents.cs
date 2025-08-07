using MS.API.Mini.Models;

namespace MS.API.Mini.Data.Models;

public class AgentSettings
{
    public string AgentID { get; set; } = string.Empty;
    
    public int AgentSleepInterval = 360;
    public int AgentAPIPort { get; set; } = 30025;
    public required string AgentVersion { get; set; }
        
    public string ProvisionedState { get; set; } = "Factory";
    public string LicenseKey { get; set; } = string.Empty;
    public required string APIBaseUrl { get; set; }
}

[Table("Agents")]
public class Agent
{
    [Required, StringLength(100)] public string AgentHostName { get; set; } = string.Empty;
        
    [Required, StringLength(100)]
    public required string AgentHostAddress { get; set; } = string.Empty;
        
    [Required, StringLength(5)]
    public required string AgentPort { get; set; }
        
    [Required, StringLength(45), JsonPropertyName("AgentID")]
    public required string AgentID { get; set; } = string.Empty;
        
    [Required, StringLength(45), JsonPropertyName("OS")]
    public required string OS { get; set; }
        
    [Required, StringLength(15), JsonPropertyName("AgentVersion")]
    public required string AgentVersion { get; set; }
        
    [Required, StringLength(25), JsonPropertyName("SDKVersion")]
    public required string SDKVersion { get; set; }
        
    public DateTime? LastSync { get; set; } = DateTime.UtcNow;
        
    [JsonPropertyName("AgentLicenseKey"), StringLength(50)]
    public string? AgentLicenseKey { get; set; }
        
    [JsonPropertyName("AgentLicenseKeyExpiryDate")]
    public DateTime? AgentLicenseKeyExpiryDate { get; set; }
        
    [JsonIgnore, JsonPropertyName("Organization"), ForeignKey("OrganizationId")]
    public int? OrganizationId { get; set; }
        
    public bool IsMonitored { get; set; }
    
    [ForeignKey("SystemMonitorId")]
    public Guid? MonitorID { get; set; }
    
    [Required, StringLength(50)]
    public required string AGENT_STATE  { get; set; }
        
    public ICollection<SystemMetric> SystemMetrics { get; set; }
    
    public ICollection<DiskData> Disks { get; set; }

    [JsonIgnore]
    public DateTime DateAdded { get; set; } = DateTime.Now;
}