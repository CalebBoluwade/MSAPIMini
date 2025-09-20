namespace MS.API.Mini.Data.Models;

public class NotificationPlatforms
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The name of the notification platform.
    /// </summary>
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The configuration for the platform, stored as JSON.
    /// </summary>
    [Required]
    public string Configuration { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public bool IsDefault { get; set; }
}

public class AlertHistory
{
    public string Id { get; set; } = string.Empty;
    
    [ForeignKey(nameof(RuleId))]
    public string RuleId { get; set; }
}