using MS.API.Mini.Configuration;
using MS.API.Mini.Contracts;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Extensions;
using MS.API.Mini.Services;
using System.Diagnostics;
using MS.API.Mini.Exceptions;

namespace MS.API.Mini.Controllers;

// [ApiController]
// [Route("api/v{version:apiVersion}/[controller]")]
public class AgentsController(
    IAnsibleDeploymentService ansibleDeploymentService,
    IAgentContract agentContract,
    GitHubService githubService,
    IOptions<AgentConfiguration> agentConfig,
    ILogger<AgentsController> logger)
    : ControllerBaseExtension
{
    [MapToApiVersion(1)]
    [HttpGet("releases")]
    public async Task<IActionResult> GetAvailableReleases()
    {
        logger.LogInformation("Fetching available agent releases from {Username}/{Repository}", 
            agentConfig.Value.GitUsername, agentConfig.Value.GitRepository);
        
        try
        {
            var releases = await githubService.GetLatestReleaseAsync(
                agentConfig.Value.GitUsername, 
                agentConfig.Value.GitRepository);
            
            logger.LogInformation("Successfully retrieved {Count} releases", releases != null ? 1 : 0);
            return Ok(releases);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch agent releases");
            return StatusCode(500, "Failed to retrieve agent releases");
        }
    }

    [MapToApiVersion(1)]
    [HttpPost("deploy")]
    public async Task<IActionResult> InitiateDeployment([FromBody] AgentDeploymentRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Initiating Agent deployment with version {Version}", 
            request.AgentVersion);
        
        try
        {
            var newAgent = await agentContract.CreateNewAgentsAsync(request, cancellationToken);
            
            var agentRemoteConfig = agentContract.CreateAgentConfiguration(new AgentSettings
            {
                LicenseKey = newAgent[0]!.AgentLicenseKey,
                AgentVersion = request.AgentVersion,
                AgentAPIPort = int.Parse(agentConfig.Value.DefaultPort),
                APIBaseUrl = agentConfig.Value.AgentRepositoryPath,
                AgentID = ""
            });
            
            logger.LogDebug("Created agent configuration with port {Port} and API URL {ApiUrl}", 
                agentConfig.Value.DefaultPort, agentConfig.Value.AgentRepositoryPath);

            var deployment = await ansibleDeploymentService.RunAnsiblePlaybookAsync(request, agentRemoteConfig);
            
            logger.LogInformation("Agent deployment initiated successfully");
            return Ok(deployment);
        }
        catch (DuplicateEntityException ex)
        {
            logger.LogWarning(ex, "Agent already exists");
            return Conflict("Agent already exists");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initiate agent deployment for version {Version}", request.AgentVersion);
            return StatusCode(500, "Failed to initiate deployment");
        }
    }

    [MapToApiVersion(1)]
    [HttpPost("validate-ssh")]
    public async Task<IActionResult> ValidateSSHConnection([FromBody] SshValidationRequest request)
    {
        logger.LogInformation("Validating SSH connection to {Host}:{Port} with user {User}", 
            request.Host, request.Port, request.Username);
        
        try
        {
            var result = await ValidateSSHAsync(request.Host, request.Port, request.Username, request.Password);
            
            logger.LogInformation("SSH validation completed for {Host} - Success: {Success}", 
                request.Host, result.IsValid);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SSH validation failed for {Host}:{Port}", request.Host, request.Port);
            return Ok(new SshValidationResult { IsValid = false, Message = "Connection failed" });
        }
    }

    private static async Task<SshValidationResult> ValidateSSHAsync(string host, int port, string username, string password)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "sshpass",
                Arguments = $"-p '{password}' ssh -o ConnectTimeout=10 -o StrictHostKeyChecking=no {username}@{host} -p {port} 'echo connected'",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var isValid = process.ExitCode == 0 && output.Contains("connected");
            var message = isValid ? "SSH connection successful" : error;

            return new SshValidationResult { IsValid = isValid, Message = message };
        }
        catch (Exception ex)
        {
            return new SshValidationResult { IsValid = false, Message = ex.Message };
        }
    }
}

public class SshValidationRequest
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SshValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
}