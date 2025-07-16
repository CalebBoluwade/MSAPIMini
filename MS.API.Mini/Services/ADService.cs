using MS.API.Mini.Data.Models;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Graph.Models;
using System.Runtime.InteropServices;
using Microsoft.Graph.Users;
using MS.API.Mini.Data;

namespace MS.API.Mini.Services;

public interface IActiveDirectoryService
{
    Task<PagedResult<ActiveDirectoryUserDTO>> GetActiveDirectoryUsersAsync(
        int page, int pageSize, string? searchTerm, bool includeDisabled);
}

public class ActiveDirectoryService(
        GraphServiceClient _graphServiceClient,
        ILogger<ActiveDirectoryService> logger)
    : IActiveDirectoryService
{
    
    public async Task<PagedResult<ActiveDirectoryUserDTO>> GetActiveDirectoryUsersAsync(
        int page, int pageSize, string? searchTerm, bool includeDisabled)
    {
        var allUsers = new List<User>();
        UserCollectionResponse? response;

        // Build the request with proper select and filter
        do
        {
            response = await _graphServiceClient.Users.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select =
                [
                    "id", "displayName", "givenName", "surname", "mail", "userPrincipalName", "department"
                    // "accountEnabled", "jobTitle", "mobilePhone", "businessPhones",
                    // "officeLocation", "companyName", "manager", "signInActivity"
                ];

                requestConfiguration.QueryParameters.Top = 999;
                requestConfiguration.QueryParameters.Orderby = ["displayName"];

                // Build filter query
                var filters = new List<string>();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    // Escape single quotes in search term
                    var escapedSearchTerm = searchTerm.Replace("'", "''");
                    filters.Add(
                        $"(startsWith(displayName,'{escapedSearchTerm}') or startsWith(givenName,'{escapedSearchTerm}') or startsWith(surname,'{escapedSearchTerm}') or startsWith(mail,'{escapedSearchTerm}'))");
                }

                // if (!includeDisabled)
                // {
                //     filters.Add("accountEnabled eq true");
                // }

                // if (filters.Count != 0)
                // {
                //     requestConfiguration.QueryParameters.Filter = string.Join(" and ", filters);
                // }

                requestConfiguration.QueryParameters.Count = true;
            });

            if (response?.Value != null)
            {
                logger.LogInformation("Found {Count} active directory users", response.Value.Count);
                allUsers.AddRange(response.Value);
            }

            // Get next page if available
            if (!string.IsNullOrEmpty(response?.OdataNextLink))
            {
                var nextPageRequest =
                    new UsersRequestBuilder(response.OdataNextLink, _graphServiceClient.RequestAdapter);
                response = await nextPageRequest.GetAsync();
            }
            else
            {
                break;
            }
        } while (response?.Value?.Count > 0);

        // Apply client-side pagination
        var totalCount = allUsers.Count;
        var skip = (page - 1) * pageSize;
        var pagedUsers = allUsers
            .Skip(skip)
            .Take(pageSize)
            .Select(MapToAdUser)
            .ToList();

        return new PagedResult<ActiveDirectoryUserDTO>
        {
            Data = pagedUsers,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    private static ActiveDirectoryUserDTO MapToAdUser(User user)
    {
        return new ActiveDirectoryUserDTO
        {
            Id = user.Id ?? string.Empty,
            DisplayName = user.DisplayName,
            GivenName = user.GivenName,
            EmailAddress = user.PostalCode,
            Department = user.Department,
            UserPrincipalName = user.UserPrincipalName,
            // Enabled = user.Enabled,
            // LastLogon = user.LastLogon,
            // Department = GetUserProperty(user, "department"),
            // Title = GetUserProperty(user, "title"),
            // PhoneNumber = GetUserProperty(user, "telephoneNumber"),
            // Manager = GetUserProperty(user, "manager")
        };
    }
}