using System.Collections.Concurrent;
using System.Diagnostics;
using LibGit2Sharp;
using MS.API.Mini.Configuration;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;

namespace MS.API.Mini.Services;

public interface IAnsibleDeploymentService
{
    Task<APIResponse<DeploymentStatus>> RunAnsiblePlaybookAsync(AgentDeploymentRequest request,
        string agentRemoteConfig);

    Task<APIResponse<DeploymentStatus>> GetDeploymentStatusAsync(string deploymentId);
}

public class AnsibleDeploymentService(
    IOptions<AgentConfiguration> _agentConfig,
    ILogger<AnsibleDeploymentService> _logger) : IAnsibleDeploymentService
{
    private readonly ConcurrentDictionary<string, DeploymentStatus> _deployments = new();
    private readonly string _tempWorkingDir = Path.Combine(Path.GetTempPath(), "ansible_deployments");

    // Get Ansible repository path from configuration
    private readonly string agentRepoPath = _agentConfig.Value.AgentRepositoryPath;

    public Task<APIResponse<DeploymentStatus>> RunAnsiblePlaybookAsync(AgentDeploymentRequest request,
        string agentRemoteConfig)
    {
        try
        {
            var deploymentId = Guid.NewGuid().ToString();
            if (request.Servers.Length == 0)
            {
                return Task.FromResult(new APIResponse<DeploymentStatus>
                {
                    Message = "Empty Server List",
                    Data = null,
                    Cause = ""
                });
            }

            // Create unique directory for this deployment
            var deploymentDir = Path.Combine(_tempWorkingDir, deploymentId);
            Directory.CreateDirectory(deploymentDir);

            // Create initial deployment status
            var status = new DeploymentStatus
            {
                Id = deploymentId,
                StartTime = DateTime.Now,
                Status = "IN_PROGRESS",
                Environment = request.Environment,
                Servers = request.Servers
            };

            _deployments[deploymentId] = status;

            // Run the Ansible playbook asynchronously
            _ = Task.Run(async () =>
            {
                try
                {
                    // Clone or update the Ansible repository if needed
                    await EnsureAnsibleRepoIsFresh(agentRepoPath, deploymentDir);

                    // Create a temporary inventory file
                    var inventoryPath = Path.Combine(deploymentDir, $"deploy/inventory_{deploymentId}.ini");
                    _logger.LogInformation("Deploying inventory to {Path}", inventoryPath);

                    await File.WriteAllTextAsync(inventoryPath,
                        BuildInventory(request.Servers, request.DeployUser, request.DeployPassword));

                    var agentConfigPath = Path.Combine(deploymentDir, "service/src/config/RemoteConfig.yml");
                    _logger.LogInformation("Deploying Config to {ConfigPath}", agentConfigPath);
                    await File.WriteAllTextAsync(agentConfigPath, agentRemoteConfig);

                    // Execute Ansible playbook
                    var processStartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = deploymentDir,
                        FileName = "ansible-playbook",
                        Arguments = $"--timeout 60 -i {inventoryPath} {deploymentDir}/deploy/deploy_agent.yml " +
                                    $"--extra-vars \"agent_version={request.AgentVersion} " +
                                    $"environment={request.Environment} " +
                                    $"agent_repo_url={agentRepoPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = new Process();
                    processStartInfo.Environment["ANSIBLE_HOST_KEY_CHECKING"] = "false";
                    process.StartInfo = processStartInfo;

                    _logger.LogInformation(
                        "Running Ansible playbook for job {DeploymentId}", deploymentId);

                    process.Start();
                    

                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data == null) return;
                        _logger.LogInformation("Ansible Output ({DeploymentId}): {Data}", deploymentId, args.Data);
                    };

                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (args.Data == null) return;
                        _logger.LogWarning("Ansible error ({DeploymentId}): {Data}", deploymentId, args.Data);
                    };

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    // Update deployment status based on the result
                    if (process.ExitCode == 0)
                    {
                        status.Status = "COMPLETED" + output;
                    }
                    else
                    {
                        status.ErrorMessage = $"Ansible process exited with code {process.ExitCode}";
                        _logger.LogError(
                            "Ansible deployment failed for job {DeploymentId} with exit code {Code}", deploymentId,
                            process.ExitCode);
                        status.Status = "FAILED";
                        status.Error = error;
                    }

                    status.EndTime = DateTime.UtcNow;
                    status.Output = output;

                    _deployments[deploymentId] = status;

                    _logger.LogInformation("Ansible playbook finished >>> {@Status}", status);

                    // Clean up the inventory file
                    try
                    {
                        // File.Delete(inventoryPath);
                    }
                    catch
                    {
                        /* Ignore errors */
                        // Clean up (or keep for debugging)
                        // Directory.Delete(deploymentDir, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during deployment {DeploymentId}", deploymentId);
                    status.Status = "FAILED";
                    status.ErrorMessage = ex.ToString();
                    status.EndTime = DateTime.UtcNow;

                    _deployments[deploymentId] = status;
                }

                _deployments.AddOrUpdate(deploymentId,
                    // Add function
                    _ => status,
                    (_, existingValue) => status);
            });

            return Task.FromResult(new APIResponse<DeploymentStatus>
            {
                Message = status.Status,
                Data = status,
                Cause = ""
            });
        }
        catch (Exception ex)
        {
            // return StatusCode(500, new { 
            //     success = false, 
            //     message = $"Deployment error: {ex.Message}",
            //     stackTrace = ex.StackTrace
            // });
            // _deployments.TryGetValue(deploymentId, out var status);
            return Task.FromResult(new APIResponse<DeploymentStatus>
            {
                Message = ex.Message,
                Data = null,
                Cause = ex.ToString()
            });
        }
    }

    private Task<string> EnsureAnsibleRepoIsFresh(string agentRepoPath, string deploymentDir)
    {
        // Clone the Ansible repository
        var cloneOptions = new CloneOptions
        {
            BranchName = "dev",
            FetchOptions =
            {
                CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials
                    {
                        Username = _agentConfig.Value.GitUsername,
                        Password = _agentConfig.Value.GitToken
                    }
            },
        };

        return Task.FromResult(Repository.Clone(agentRepoPath, deploymentDir, cloneOptions));
    }

    public Task<APIResponse<DeploymentStatus>> GetDeploymentStatusAsync(string deploymentId)
    {
        _deployments.TryGetValue(deploymentId, out var status);

        var response = new APIResponse<DeploymentStatus>
        {
            Message = "Deployment status retrieved",
            Data = status!,
        };
        return Task.FromResult(response)!;
    }

    private static string BuildInventory(IEnumerable<string> servers, string deployUser, string deployPassword)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[agents]");
        foreach (var server in servers)
        {
            sb.AppendLine(server);
        }

        sb.AppendLine();
        sb.AppendLine("[agents:vars]");
        sb.AppendLine(";ansible_python_interpreter=/usr/bin/python3");

        sb.AppendLine();
        sb.AppendLine("[all:vars]");
        sb.AppendLine($"ansible_ssh_user={deployUser}");
        sb.AppendLine($"ansible_ssh_pass={deployPassword}");
        sb.AppendLine($"ansible_become_pass={deployPassword}");
        sb.AppendLine("ansible_become=yes");
        
        return sb.ToString();
    }
}