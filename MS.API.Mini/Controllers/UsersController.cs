using Microsoft.Graph;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Extensions;
using MS.API.Mini.Services;

namespace MS.API.Mini.Controllers;

public class UsersController(ILogger<UsersController> _logger, IActiveDirectoryService activeDirectoryService)
    : ControllerBaseExtension
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ActiveDirectoryUserDTO>>> GetUsers(
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
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Microsoft Graph API error: {Error}", ex.Message);
            return StatusCode(500, $"Graph API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching AD users");
            return StatusCode(500, "Internal server error while fetching users");
        }
    }
}