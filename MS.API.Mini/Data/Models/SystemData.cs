namespace MS.API.Mini.Data.Models
{
    public struct EntityType
    {
        public const string SERVER = "SERVER";
        public const string MSSQL = "MSSQL";
    }
    
    [Table("SystemMetricData")]
    public class SystemMetric
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        
        [ForeignKey(nameof(SystemMonitorId))]
        public Guid SystemMonitorId { get; set; } // Foreign key
        
        public long? Timestamp { get; set; }
        public required string AgentID { get; set; }
        public long? TimestampMem { get; set; }
        public double CPUUsage { get; set; }
        public double MemoryUsage { get; set; }
        
        [Required, NotMapped, JsonPropertyName("Agent Name")]
        public string AgentName { get; set; }
    
        [Required, NotMapped, JsonPropertyName("Agent Operating System")]
        public string AgentOS { get; set; }
    
        [Required, NotMapped, JsonPropertyName("Agent Version")]
        public string AgentVersion { get; set; }
        
        // Navigation property
        public SystemMonitor SystemMonitor { get; set; } // Navigation property
        public Agent Agent { get; set; }
    }

    [Table("SystemDiskData"), Keyless]
    public class DiskData
    {
        [ForeignKey("AgentId"), StringLength(55)]
        public required string AgentID { get; set; }
        
        [JsonPropertyName("Drive"), Required, StringLength(75)]
        public string? Drive { get; set; }

        [JsonPropertyName("FreeSpaceFormatted"), StringLength(10)]
        public string? FormatFree { get; set; }

        [JsonPropertyName("DiskSizeFormatted"), StringLength(10)]
        public string? FormatSize { get; set; }

        [JsonPropertyName("FreeSpaceUnformatted")]
        public long? Free { get; set; }

        [JsonPropertyName("DiskSize")]
        public long? Size { get; set; }

        [JsonPropertyName("Used"), Column(TypeName = "decimal(18,6)")]
        public decimal? Used { get; set; }
        
        public virtual Agent Agent { get; set; } = null!;
    }
    
    [Table("NetworkDeviceMetricData"), Keyless]
    public class NetworkDeviceMetric
    {
        public Guid SystemMonitorId { get; set; }
        public required string DeviceName { get; set; }
        public required string DeviceIP { get; set; }
        public required string MetricName { get; set; }
        public required string MetricDescription { get; set; }
        public required string MetricValue { get; set; }
        public DateTime LastPoll { get; set; }
    }

}
