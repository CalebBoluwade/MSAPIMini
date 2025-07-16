namespace MS.API.Mini.Data.Models;

public class ActiveDirectoryUserDTO
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;

    public string Department { get; set; }
    public string UserPrincipalName { get; set; } = string.Empty;

    public string EmailAddress { get; set; } = string.Empty;
    // Add other properties you need
}