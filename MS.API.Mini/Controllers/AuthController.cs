using MS.API.Mini.Data.Models;
using MS.API.Mini.Extensions;
using MS.API.Mini.Services;

namespace MS.API.Mini.Controllers;

public class AuthController(IActiveDirectoryService activeDirectoryService, ILogger<AuthController> logger)
    : ControllerBaseExtension
{
    [MapToApiVersion(1)]
    // [ResponseType(typeof(UserAuthLoginResponse))]
    [HttpPost(nameof(AuthenticateUserAD))]
    public async Task<ActionResult<UserAuthLoginResponse>> AuthenticateUserAD(
        [FromBody] UserAuthLoginRequest request)
    {
        try
        {
            var result = await activeDirectoryService.AuthenticateUserAsync(request.Username, request.Password);

            if (result.IsAuthenticated)
            {
                logger.LogInformation("User {Username} authenticated successfully", request.Username);
                return Ok(result);
            }
            else
            {
                logger.LogWarning("Authentication failed for user {Username}: {Message}", request.Username,
                    result.Message);
                return Unauthorized(result);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login attempt for user {Username}", request.Username);
            return StatusCode(500, new UserAuthLoginResponse
            {
                IsAuthenticated = false,
                Message = "An internal error occurred during authentication."
            });
        }
    }
}