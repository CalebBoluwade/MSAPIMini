using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Extensions;
using MS.API.Mini.Services;

namespace MS.API.Mini.Controllers;

public class UsersController(ILogger<UsersController> logger, IActiveDirectoryService activeDirectoryService)
    : ControllerBaseExtension
{
    [HttpGet("u")]
    public async Task<ActionResult<List<ActiveDirectoryUserMiniDTO>>> GetAllUsers()
    {
        try
        {
            var users = await activeDirectoryService.GetActiveDirectoryUsersAsync(null, null, null, false);
            logger.LogInformation("Retrieved {UserCount} users from LDAP", users.Count);
            return Ok(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all users from LDAP");
            return StatusCode(500, new { Message = "An error occurred while retrieving users." });
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<ActiveDirectoryUserMiniDTO>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool includeDisabled = false)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 999) pageSize = 50; // Graph API max is 999

            var result =
                await activeDirectoryService.GetActiveDirectoryUsersAsync(page, pageSize, searchTerm, includeDisabled);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching AD users");
            return StatusCode(500, "Internal server error while fetching users");
        }
    }
}