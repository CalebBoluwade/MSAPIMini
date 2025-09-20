using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Extensions;
using Novell.Directory.Ldap;

namespace MS.API.Mini.Services;

/// <summary>
/// Maps to the "Ldap" configuration section in appsettings.json.
/// </summary>
public class LdapConfig
{
    public required string Server { get; set; }
    public int Port { get; set; }
    public required string BindDn { get; set; }
    public required string BindPassword { get; set; }
    public required string SearchBase { get; set; }
    public required string DomainName { get; set; }
    public bool UseSSL { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}

public interface IActiveDirectoryService
{
    /// <summary>
    /// Authenticates a user against the LDAP server.
    /// </summary>
    /// <param name="username">The user's username.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>True if authentication is successful, otherwise false.</returns>
    Task<UserAuthLoginResponse> AuthenticateUserAsync(string username, string password);

    Task<List<ActiveDirectoryUserMiniDTO>> GetActiveDirectoryUsersAsync(
        int page, int pageSize, string? searchTerm, bool includeDisabled);

    Task<List<ActiveDirectoryUserMiniDTO>> GetAllActiveDirectoryUsersAsync(
        int page, int pageSize, bool includeDisabled);

    Task<List<ActiveDirectoryUserMiniDTO>> GetUsersByRuleIdAsync(Guid ruleId);
}

public class ActiveDirectoryService(
    IOptions<LdapConfig> config,
    MonitorDBContext dbCtx,
    ILogger<ActiveDirectoryService> logger)
    : IActiveDirectoryService
{
    /// <summary>
    /// Authenticates a user using the Novell.Directory.Ldap library.
    /// </summary>
    public async Task<UserAuthLoginResponse> AuthenticateUserAsync(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            logger.LogWarning("Authentication failed: Username or password is empty.");
            return new UserAuthLoginResponse
            {
                IsAuthenticated = false,
                Message = "Username and password are required."
            };
        }

        try
        {
            using var connection = new LdapConnection();

            // Set timeout
            connection.Constraints.TimeLimit = config.Value.TimeoutSeconds * 1000;

            // Connect to LDAP server
            logger.LogInformation("Connecting to LDAP server: {Server}:{Port}", config.Value.Server, config.Value.Port);
            await Task.Run(() => connection.Connect(config.Value.Server, config.Value.Port));

            // For production, always use SSL/TLS. The server certificate must be trusted.
            if (config.Value.UseSSL)
            {
                connection.SecureSocketLayer = true;
                connection.StartTls();
            }

            // The User Principal Name (UPN) is often used for binding in AD environments.
            var userPrincipalName = username.Contains('@') ? username : $"{username}@{config.Value.DomainName}";

            logger.LogInformation("Attempting to bind user: {UserPrincipalName}", userPrincipalName);
            // Attempt to bind (authenticate) with user credentials
            // Bind to the directory with the user's credentials.
            // If the bind is successful, the credentials are valid.
            await Task.Run(() => connection.Bind(LdapConnection.LdapV3, userPrincipalName, password));

            var user = await GetUserDetailsAsync(connection, userPrincipalName);
            logger.LogInformation("User '{UserPrincipalName}' authenticated successfully with Details {@User}.",
                userPrincipalName, user);

            return new UserAuthLoginResponse
            {
                IsAuthenticated = true,
                Message = "Authentication successful.",
                UserData = user
            };
        }
        catch (LdapException ex)
        {
            // Common error codes: 49 (InvalidCredentials), 50 (InsufficientAccessRights)
            logger.LogError(ex, "LDAP authentication failed for user '{Username}'. LDAP Error Code: {ResultCode}",
                username, ex.ResultCode);
            return new UserAuthLoginResponse
            {
                IsAuthenticated = false,
                Message = ex.ResultCode switch
                {
                    LdapException.InvalidCredentials => "Invalid username or password.",
                    LdapException.ConnectError => "Unable to Connect to Authentication Server.",
                    LdapException.TimeLimitExceeded => "Authentication request timed out.",
                    _ => "Authentication failed."
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during LDAP authentication for user '{Username}'.",
                username);
            return new UserAuthLoginResponse
            {
                IsAuthenticated = false,
                Message = "An unexpected error occurred during authentication."
            };
        }
    }
    
    public async Task<List<ActiveDirectoryUserMiniDTO>> GetAllActiveDirectoryUsersAsync(
        int page, int pageSize, bool includeDisabled)
    {
        var allUsers = new List<ActiveDirectoryUserMiniDTO>();

        try
        {
            using var connection = new LdapConnection();
            connection.Constraints.TimeLimit = config.Value.TimeoutSeconds * 1000;

            // Connect and bind with admin credentials
            await Task.Run(() => connection.Connect(config.Value.Server, config.Value.Port));

            if (config.Value.UseSSL)
            {
                connection.StartTls();
            }

            logger.LogInformation("Attempting to bind user: {AdminDN}", config.Value.BindDn);

            await Task.Run(() =>
                connection.Bind(LdapConnection.LdapV3, config.Value.BindDn, config.Value.BindPassword));

            // Search for all user objects
            const string filter = $"(&(objectClass=user)(objectCategory=person))";
            string[] attributes =
            [
                "id", "sAMAccountName", "displayName", "mail", "givenName", "sn",
                "department", "title", "telephoneNumber", "distinguishedName", "memberOf"
            ];
            // "accountEnabled", "jobTitle", "mobilePhone", "businessPhones", "userPrincipalName"
            // "officeLocation", "companyName", "manager", "signInActivity"

            var searchResults = await Task.Run(() =>
                connection.Search(config.Value.SearchBase, LdapConnection.ScopeSub, filter, attributes, false));

            while (searchResults.HasMore())
            {
                try
                {
                    var entry = searchResults.Next();
                    var user = await MapLDAPEntryMiniToUser(entry);

                    allUsers.Add(user);
                }
                catch (LdapException ex)
                {
                    logger.LogWarning(ex, "Error processing LDAP entry");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users from LDAP");
        }

        return allUsers;
    }

    public async Task<List<ActiveDirectoryUserMiniDTO>> GetActiveDirectoryUsersAsync(
        int page, int pageSize, string? searchTerm, bool includeDisabled)
    {
        var allUsers = new List<ActiveDirectoryUserMiniDTO>();

        try
        {
            using var connection = new LdapConnection();
            connection.Constraints.TimeLimit = config.Value.TimeoutSeconds * 1000;

            // Connect and bind with admin credentials
            await Task.Run(() => connection.Connect(config.Value.Server, config.Value.Port));

            if (config.Value.UseSSL)
            {
                connection.StartTls();
            }

            logger.LogInformation("Attempting to bind user: {AdminDN}", config.Value.BindDn);

            await Task.Run(() =>
                connection.Bind(LdapConnection.LdapV3, config.Value.BindDn, config.Value.BindPassword));

            var dbUsers = await dbCtx.Users
                .Skip(page * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            if (dbUsers.Count == 0) return allUsers;

            logger.LogInformation("FA {@Users} \n", dbUsers);

            var userFilters = dbUsers.Select(user => $"(userPrincipalName={user.WorkEmail})");

            // Search for all user objects
            var filter = $"(&(objectClass=user)(objectCategory=person)(|{string.Join("", userFilters)}))";
            string[] attributes =
            [
                "id", "sAMAccountName", "displayName", "mail", "givenName", "sn",
                "department", "title", "telephoneNumber", "distinguishedName", "memberOf"
            ];
            // "accountEnabled", "jobTitle", "mobilePhone", "businessPhones", "userPrincipalName"
            // "officeLocation", "companyName", "manager", "signInActivity"

            var searchResults = await Task.Run(() =>
                connection.Search(config.Value.SearchBase, LdapConnection.ScopeSub, filter, attributes, false));

            while (searchResults.HasMore())
            {
                try
                {
                    var entry = searchResults.Next();
                    var user = await MapLDAPEntryMiniToUser(entry);

                    allUsers.Add(user);
                }
                catch (LdapException ex)
                {
                    logger.LogWarning(ex, "Error processing LDAP entry");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users from LDAP");
        }

        return allUsers;
    }

    public async Task<List<ActiveDirectoryUserMiniDTO>> GetUsersByRuleIdAsync(Guid ruleId)
    {
        var users = new List<ActiveDirectoryUserMiniDTO>();

        try
        {
            var recipientUserIds = await dbCtx.MonitoringRules
                .Where(r => r.Id == ruleId)
                .Select(r => r.RecipientUserIds)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (recipientUserIds == null || recipientUserIds.Length == 0) return users;

            var workEmails = await dbCtx.Users
                .Where(u => recipientUserIds.Contains(u.UserId.ToString()))
                .Select(u => u.WorkEmail)
                .AsNoTracking()
                .ToListAsync();

            if (workEmails.Count == 0) return users;

            using var connection = new LdapConnection();
            connection.Constraints.TimeLimit = config.Value.TimeoutSeconds * 1000;

            await Task.Run(() => connection.Connect(config.Value.Server, config.Value.Port));

            if (config.Value.UseSSL)
            {
                connection.StartTls();
            }

            await Task.Run(() =>
                connection.Bind(LdapConnection.LdapV3, config.Value.BindDn, config.Value.BindPassword));

            var userFilters = workEmails.Select(email => $"(userPrincipalName={email})");
            var filter = $"(&(objectCategory=user)(|{string.Join("", userFilters)}))";

            string[] attributes = ["sAMAccountName", "displayName", "mail", "title", "telephoneNumber", "memberOf"];

            var searchResults = await Task.Run(() =>
                connection.Search(config.Value.SearchBase, LdapConnection.ScopeSub, filter, attributes, false));

            while (searchResults.HasMore())
            {
                try
                {
                    var entry = searchResults.Next();
                    var user = await MapLDAPEntryMiniToUser(entry);
                    if (user != null) users.Add(user);
                }
                catch (LdapException ex)
                {
                    logger.LogWarning(ex, "Error processing LDAP entry");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users by rule ID: {RuleId}", ruleId);
        }

        return users;
    }

    private async Task<ActiveDirectoryUserDTO?> GetUserDetailsAsync(LdapConnection connection, string username)
    {
        try
        {
            var filter = $"(&(objectClass=user)(userPrincipalName={username}))";
            logger.LogInformation("Fetching User Info using {Filter}", filter);
            string[] attributes =
            [
                "sAMAccountName", "displayName", "mail", "thumbnailPhoto", "jpegPhoto", "title", "telephoneNumber",
                "memberOf", "dn", "distinguishedName", "extensionAttribute8",
                "extensionAttribute12",
                "extensionAttribute2",
                "extensionAttribute10",
            ];

            var searchResults = await Task.Run(() =>
                connection.Search(config.Value.SearchBase, LdapConnection.ScopeSub, filter, attributes, false));

            if (!searchResults.HasMore()) return null;
            var entry = searchResults.Next();
            return MapLDAPEntryToUser(entry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user details for {Username}", username);
            return null;
        }
    }

    private async Task<ActiveDirectoryUserMiniDTO?> MapLDAPEntryMiniToUser(LdapEntry userEntry)
    {
        var dbUser = await dbCtx.Users.FirstOrDefaultAsync(u => u.WorkEmail == GetAttributeValue(userEntry, "mail"));
        if (dbUser == null)
        {
            logger.LogWarning("User not found in database: {User}", GetAttributeValue(userEntry, "mail"));
            return null;
        }

        var userDTO = new ActiveDirectoryUserMiniDTO
        {
            Id = dbUser.UserId,
            Avatar = GetUserPhotoBase64(userEntry),
            FullName = GetAttributeValue(userEntry, "displayName"),
            EmailAddress = GetAttributeValue(userEntry, "mail"),
            Title = GetAttributeValue(userEntry, "title"),
            PhoneNumber = GetAttributeValue(userEntry, "telephoneNumber").SanitizePhoneNumber(),
        };

        return userDTO;
    }

    private static ActiveDirectoryUserDTO MapLDAPEntryToUser(LdapEntry userEntry)
    {
        var userDTO = new ActiveDirectoryUserDTO
        {
            // Id = user.Id ?? string.Empty,
            Avatar = GetUserPhotoBase64(userEntry),
            UserPrincipalName = GetAttributeValue(userEntry, "distinguishedName"),
            Username = GetAttributeValue(userEntry, "sAMAccountName"),
            FullName = GetAttributeValue(userEntry, "displayName"),
            EmailAddress = GetAttributeValue(userEntry, "mail"),
            FirstName = GetAttributeValue(userEntry, "givenName"),
            LastName = GetAttributeValue(userEntry, "sn"),
            Department = GetAttributeValue(userEntry, "department"),
            Title = GetAttributeValue(userEntry, "title"),
            PhoneNumber = GetAttributeValue(userEntry, "telephoneNumber").SanitizePhoneNumber(),
            Manager = GetAttributeValue(userEntry, "manager")
        };

        var memberOfAttribute = userEntry.GetAttribute("memberOf");
        if (memberOfAttribute == null) return userDTO;
        var groups = memberOfAttribute.StringValueArray;
        userDTO.Groups = groups?.Select(ExtractGroupName).Where(g => !string.IsNullOrEmpty(g)).ToList() ?? [];

        return userDTO;
    }

    private static string GetAttributeValue(LdapEntry entry, string attributeName)
    {
        try
        {
            var attribute = entry.GetAttribute(attributeName);
            return attribute?.StringValue ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ExtractGroupName(string distinguishedName)
    {
        // Extract CN from group DN (e.g., "CN=GroupName,OU=Groups,DC=domain,DC=com" -> "GroupName")
        if (string.IsNullOrEmpty(distinguishedName)) return string.Empty;

        var cnIndex = distinguishedName.IndexOf("CN=", StringComparison.OrdinalIgnoreCase);
        if (cnIndex == -1) return string.Empty;

        var start = cnIndex + 3;
        var commaIndex = distinguishedName.IndexOf(',', start);

        return commaIndex == -1 ? distinguishedName[start..] : distinguishedName.Substring(start, commaIndex - start);
    }

    private static string GetUserPhotoBase64(LdapEntry entry)
    {
        var attributes = entry.GetAttributeSet();

        // Check for thumbnailPhoto
        if (attributes.ContainsKey("thumbnailPhoto"))
        {
            // Try thumbnailPhoto first
            var photoAttr = entry.GetAttribute("thumbnailPhoto");
            if (photoAttr is { ByteValue.Length: > 0 })
                return Convert.ToBase64String(photoAttr.ByteValue);
        }

        // Fallback: jpegPhoto
        if (!attributes.ContainsKey("jpegPhoto")) return string.Empty;
        var jpegAttr = entry.GetAttribute("jpegPhoto");
        return jpegAttr is { ByteValue.Length: > 0 } ? Convert.ToBase64String(jpegAttr.ByteValue) : string.Empty;
    }
}