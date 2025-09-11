using MS.API.Mini.Configuration;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Exceptions;
using MS.API.Mini.Extensions;

namespace MS.API.Mini.Contracts;

public interface IAgentContract
{
    public string GenerateAgentLicenseKey();

    public string CreateAgentConfiguration(AgentSettings agentSettings);

    Task<List<Agent>> CreateNewAgentsAsync(AgentDeploymentRequest request, CancellationToken cancellationToken);
}

public class AgentContractor(
    MonitorDBContext dbCtx,
    ILogger<AgentContractor> logger,
    IOptions<AgentConfiguration> agentConfiguration)
    : IAgentContract
{
    public string GenerateAgentLicenseKey()
    {
        return Guid.NewGuid().ToString("N");
    }

    public string CreateAgentConfiguration(AgentSettings agentSettings)
    {
        agentSettings.LicenseKey ??= GenerateAgentLicenseKey();

        var properties = agentSettings.GetType().GetProperties();
        var envString = new List<string>();

        foreach (var prop in properties)
        {
            var name = ToEnvKey(prop.Name);
            var value = prop.GetValue(agentSettings)?.ToString() ?? string.Empty;

            // Escape special characters if necessary (e.g., newlines, quotes)
            if (value.Contains(" ") || value.Contains("\""))
            {
                value = $"\"{value.Replace("\"", "\\\"")}\"";
            }

            envString.Add($"{name}={value}");
        }

        return string.Join(Environment.NewLine, envString);
    }

    private static string ToEnvKey(string input)
    {
        // Convert PascalCase or camelCase to UPPER_CASE
        return string.Concat(
            input.Select((c, i) =>
                i > 0 && char.IsUpper(c) ? $"_{c}" : c.ToString()
            )
        ).ToUpper();
    }

    public async Task<List<Agent>> CreateNewAgentsAsync(AgentDeploymentRequest request,
        CancellationToken cancellationToken)
    {
        var existingAppEntities = await dbCtx.SystemMonitors
            .Where(x => request.Servers.Contains(x.IPAddress) && string.IsNullOrEmpty(x.Agent))
            .ToListAsync(cancellationToken);

        var existingAgents = await dbCtx.Agents.Where(x => request.Servers.Contains(x.AgentHostAddress))
            .ToListAsync(cancellationToken);

        // If profiling already exists in either ApplicationEntities or Agents, do not proceed
        if (existingAppEntities.Count != 0 || existingAgents.Count != 0)
        {
            // Agent already profiled, return null or handle accordingly
            throw new DuplicateEntityException();
        }

        var createdAgents = new List<Agent>();

        // Otherwise, create and save new agents for each server
        foreach (var server in request.Servers)
        {
            var newAgent = new Agent
            {
                // Populate properties from the request
                AGENT_STATE = "IN PROGRESS",
                AgentPort = request.AgentPort ?? agentConfiguration.Value.DefaultPort,
                AgentHostAddress = server,
                AgentID = GenerateAgentLicenseKey(),
                AgentHostName = "",
                AgentVersion = request.AgentVersion,
                OS = "",
                AgentLicenseKey = "",
                MonitorID = request.SystemMonitorId,
                SDKVersion = "",
                AgentLicenseKeyExpiryDate = DateTime.UtcNow.AddDays(30),
            };

            // Check if SystemMonitor exists, update or add accordingly
            var existingSystemMonitor = await dbCtx.SystemMonitors
                .FirstOrDefaultAsync(x => x.IPAddress == server && x.SystemMonitorId == request.SystemMonitorId,
                    cancellationToken);

            if (existingSystemMonitor != null)
            {
                existingSystemMonitor.Agent = newAgent.AgentID;
                dbCtx.SystemMonitors.Update(existingSystemMonitor);
            }
            else
            {
                var newSystemMonitor = new SystemMonitor
                {
                    IPAddress = server,
                    Agent = newAgent.AgentID,
                    SystemMonitorId = request.SystemMonitorId
                };
                dbCtx.SystemMonitors.Add(newSystemMonitor);
            }

            dbCtx.Agents.Add(newAgent);
            createdAgents.Add(newAgent);
        }

        await dbCtx.SaveChangesAsync(cancellationToken);
        return createdAgents;
    }
}