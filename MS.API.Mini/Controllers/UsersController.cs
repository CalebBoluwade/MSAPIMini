using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Extensions;
using MS.API.Mini.Services;

namespace MS.API.Mini.Controllers;

public class UsersController(ILogger<UsersController> logger, IActiveDirectoryService activeDirectoryService)
    : ControllerBaseExtension
{
    [HttpGet("all")]
    public async Task<ActionResult<PagedResult<ActiveDirectoryUserMiniDTO>>> GetAllUsers(
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool includeDisabled = false)
    {
        try
        {
            if (page < 0) page = 0;
            if (pageSize is < 1 or > 999) pageSize = 50;

            var result =
                await activeDirectoryService.GetAllActiveDirectoryUsersAsync(page, pageSize, includeDisabled);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching AD users");
            return StatusCode(500, "Internal server error while fetching users");
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<ActiveDirectoryUserMiniDTO>>> GetUsers(
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool includeDisabled = false)
    {
        try
        {
            if (page < 0) page = 0;
            if (pageSize is < 1 or > 999) pageSize = 50;

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
    
    [HttpGet("{ruleId:guid}")]
    public async Task<ActionResult<PagedResult<ActiveDirectoryUserMiniDTO>>> GetRuleRecipients([Required, FromRoute] Guid ruleId)
    {
        try
        {
            logger.LogInformation("Retrieving users for rule {RuleId}", ruleId);
            
            var result =
                await activeDirectoryService.GetUsersByRuleIdAsync(ruleId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching AD users");
            return StatusCode(500, "Internal server error while fetching users");
        }
    }
}