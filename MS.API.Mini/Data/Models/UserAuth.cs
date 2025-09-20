namespace MS.API.Mini.Data.Models;

/// <summary>
/// Represents the data required for a login request.
/// </summary>
public class UserAuthLoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class UserAuthLoginResponse
{
    public bool IsAuthenticated { get; set; }
    public string Message { get; set; } = string.Empty;
    public ActiveDirectoryUserDTO? UserData { get; set; }
}

[Table("Users")]
public sealed class DBUser
{
    [Key]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(75)]
    // [NotMapped]
    public required string FullName
    {
        get; set;
    }
    
    [Required, EmailAddress, MaxLength(75)]
    public required string WorkEmail { get; set; }

    [ForeignKey("OrganizationId")]
    public Guid OrganizationId { get; set; }

    [Required, MaxLength(150), PasswordPropertyText]
    public string MFASecret { get; set; } = string.Empty;

    public bool IsMFAEnabled { get; set; } = false;

    [Timestamp]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}