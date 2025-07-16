using MS.API.Mini.Data.Models;

namespace MS.API.Mini.Models;

public class NotificationGroup
{
    [Key] public int Id { get; set; }

    [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;

    public ICollection<UserNotificationGroup> UserNotificationGroups { get; set; } = new List<UserNotificationGroup>();

    public ICollection<ServiceNotificationGroup> ServiceNotificationGroups { get; set; } =
        new List<ServiceNotificationGroup>();

    public ICollection<GroupNotificationPlatforms> GroupNotificationPlatforms { get; set; } =
        new List<GroupNotificationPlatforms>();
}

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

    public bool IsEnabled { get; set; } = false;

    public bool IsDefault { get; set; } = false;

    public ICollection<GroupNotificationPlatforms> GroupNotificationPlatforms { get; set; } =
        new List<GroupNotificationPlatforms>();
}

public class UserNotificationGroup
{
    [Key] public int Id { get; set; }

    [Required] public long UserId { get; set; }

    [Required] public int GroupId { get; set; }

    [ForeignKey(nameof(UserId))]
    public DBUser User { get; set; } = new()
    {
        WorkEmail = string.Empty,
        FullName = string.Empty,
    };

    [ForeignKey(nameof(GroupId))] public NotificationGroup Group { get; set; } = new();
}

public class ServiceNotificationGroup
{
    [Key] public int Id { get; set; }

    [Required] public Guid SystemMonitorId { get; set; }

    [Required, ForeignKey(nameof(GroupId))]
    public int GroupId { get; set; }

    [ForeignKey(nameof(SystemMonitorId))] public virtual SystemMonitor Service { get; set; } = new();
}

public class GroupNotificationPlatforms
{
    [Key] public int Id { get; set; }

    [Required] public int GroupId { get; set; }

    [Required] public int PlatformId { get; set; }

    [ForeignKey(nameof(GroupId))] public virtual NotificationGroup Group { get; set; } = new();

    [ForeignKey(nameof(PlatformId))] public virtual NotificationPlatforms Platform { get; set; } = new();
}