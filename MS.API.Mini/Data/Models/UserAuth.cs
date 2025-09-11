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
    public long Id { get; set; }
    
    [Required]
    [MaxLength(75)]
    // [NotMapped]
    public required string FullName
    {
        
        get; set;
    }

    [Required, MaxLength(150), PasswordPropertyText]
    public string Password { get; set; }

    [MaxLength(15), Phone]
    public string? PhoneNumber { get; set; }

    [Required, EmailAddress, MaxLength(75)]
    public required string WorkEmail { get; set; }

    [ForeignKey("OrganizationId")]
    public int OrganizationId { get; set; }

    public string Role { get; set; } = string.Empty;

    [Required, MaxLength(150), PasswordPropertyText]
    public string MFASecret { get; set; } = string.Empty;

    public bool IsMFAEnabled { get; set; } = false;

    [Timestamp]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}