namespace MS.API.Mini.Data.Models;

public class ActiveDirectoryUserDTO
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public List<string> Groups { get; set; } = [];
    
    public string Manager { get; set; } = string.Empty;
    
    public string Avatar { get; set; } = string.Empty;
    
    public string UserPrincipalName { get; set; } = string.Empty;

    public string EmailAddress { get; set; } = string.Empty;

}

public class ActiveDirectoryUserMiniDTO
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public List<string> Groups { get; set; } = [];
    
    public string Avatar { get; set; } = string.Empty;

    public string EmailAddress { get; set; } = string.Empty;

}