namespace MS.API.Mini.Models;

[Table("Agents")]
public class Agents
{
    [Required, StringLength(100)] public string AgentHostName { get; set; } = string.Empty;
        
    [Required, StringLength(100)]
    public required string AgentHostAddress { get; set; } = string.Empty;
        
    [Required, StringLength(5)]
    public required string AgentPort { get; set; }
        
    [Required, StringLength(25), JsonPropertyName("AgentID")]
    public required string AgentID { get; set; } = string.Empty;
        
    [Required, StringLength(45), JsonPropertyName("OS")]
    public required string OS { get; set; }
        
    [Required, StringLength(15), JsonPropertyName("AgentVersion")]
    public required string AgentVersion { get; set; }
        
    [Required, StringLength(10), JsonPropertyName("SDKVersion")]
    public required string SDKVersion { get; set; }
        
    public DateTime LastSync { get; set; } = DateTime.UtcNow;
        
    [JsonPropertyName("AgentLicenseKey"), Required, StringLength(50)]
    public required string AgentLicenseKey { get; set; }
        
    [JsonPropertyName("AgentLicenseKeyExpiryDate"), Required]
    public required DateTime AgentLicenseKeyExpiryDate { get; set; }
        
    [Required]
    [ForeignKey("OrganizationId")]
    [JsonIgnore, JsonPropertyName("Organization")]
    public int OrganizationId { get; set; }
        
    public bool VP { get; set; }
        
    public required Guid AppOwnerID { get; set; }
        
    public required string AGENT_STATE  { get; set; }
        
    // public ICollection<SystemMetric> SystemMetrics { get; set; }
    // public ICollection<DiskData> Disks { get; set; }

    [JsonIgnore]
    public DateTime DateAdded { get; set; } = DateTime.Now;
}