using MS.API.Mini.Configuration;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;

namespace MS.API.Mini.Contracts;

public interface IAgentContract
{
    public string GenerateAgentLicenseKey();
    
    public string CreateAgentConfiguration(AgentSettings agentSettings);
    
    Task<Agent> CreateNewAgentAsync(AgentDeploymentRequest request, CancellationToken cancellationToken);
}

public class AgentContractor(MonitorDBContext dbCtx, ILogger<AgentContractor> logger, IOptions<AgentConfiguration> agentConfiguration)
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

    public async Task<Agent> CreateNewAgentAsync(AgentDeploymentRequest request, CancellationToken cancellationToken)
    {
       var existingAppEntities = await dbCtx.SystemMonitors.Where(x => request.Servers.Contains(x.IPAddress) && string.IsNullOrEmpty(x.Agent)).ToListAsync(cancellationToken);
       
       var existingAgents = await dbCtx.Agents.Where(x => request.Servers.Contains(x.AgentHostAddress)).ToListAsync(cancellationToken);
       
       // If profiling already exists in either ApplicationEntities or Agents, do not proceed
       if (existingAppEntities.Count != 0 || existingAgents.Count != 0)
       {
           // Agent already profiled, return null or handle accordingly
           return null;
       }
       
       // Otherwise, create and save a new agent
       var newAgent = new Agent
       {
           // Populate properties from the request
           AGENT_STATE = "IN PROGRESS",
           AgentPort = request.AgentPort ?? agentConfiguration.Value.DefaultPort,
           AgentHostAddress = request.Servers.First(), // Example, adjust as needed
           AgentID = request.AgentID,
           AgentHostName = "",
           AgentVersion = request.AgentVersion,
           OS = "",
           AgentLicenseKey = "",
           MonitorID = request.AppOwnerID,
           SDKVersion = "",
           AgentLicenseKeyExpiryDate = DateTime.UtcNow.AddDays(30),
       };

       var updateEntity = new SystemMonitor
       {
           IPAddress = request.Servers.First(),
           Agent = newAgent.AgentID,
           SystemMonitorId = request.AppOwnerID
       };

       dbCtx.Agents.Add(newAgent);
       
       dbCtx.SystemMonitors.Update(updateEntity);
       await dbCtx.SaveChangesAsync(cancellationToken);

       return newAgent;
    }
}