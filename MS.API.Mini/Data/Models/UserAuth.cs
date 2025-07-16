namespace MS.API.Mini.Models;

public class UserAuth
{
    
}

[Table("Users")]
public sealed class DBUser
{
    public long Id { get; set; }
    
    [Required]
    [MaxLength(75)]
    // [NotMapped]
    public required string FullName
    {
        // get { return $"{FirstName} {LastName}"; }
        get; set;
    }

    [Required, MaxLength(150), PasswordPropertyText]
    public string Password { get; set; }

    [MaxLength(15), Phone]
    public string? PhoneNumber { get; set; }

    [Required, EmailAddress, MaxLength(75)]
    public required string WorkEmail { get; set; }
        
    public ICollection<UserNotificationGroup> UserNotificationGroups { get; set; } = new List<UserNotificationGroup>();
        
    // Navigation collection to ServiceNotificationGroups if needed
    public ICollection<ServiceNotificationGroup> ServiceNotificationGroups { get; set; } = new List<ServiceNotificationGroup>();

    [ForeignKey("OrganizationId")]
    public int OrganizationId { get; set; }

    public string Role { get; set; } = string.Empty;

    [Required, MaxLength(150), PasswordPropertyText]
    public string MFASecret { get; set; } = string.Empty;

    public bool IsMFAEnabled { get; set; } = false;

    [Timestamp]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}