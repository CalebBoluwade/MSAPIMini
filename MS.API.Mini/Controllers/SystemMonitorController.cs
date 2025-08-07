using System.Text.Json;
using Asp.Versioning;
using MS.API.Mini.Configuration;
using MS.API.Mini.Contracts;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Extensions;
using MS.API.Mini.Services;

namespace MS.API.Mini.Controllers;

public class SystemMonitorController(MonitorDBContext _dbCtx,
    IAnsibleDeploymentService _ansibleDeploymentService,
    IAgentContract agentContractor,
    GitHubService _githubService,
    IOptions<AgentConfiguration> _agentConfig,
    ILogger<SystemMonitorController> logger)
    : ControllerBaseExtension
{
    [MapToApiVersion(1)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SystemMonitorDTO>>> GetSystemMonitors()
    {
        var results = await _dbCtx.SystemMonitors
            .AsSplitQuery()
            .OrderBy(x => x.CreatedAt)
            .Select(m => new SystemMonitorDTO
            {
                SystemMonitorId = m.SystemMonitorId,
                IPAddress = m.IPAddress,
                ServiceName = m.ServiceName,
                Description = m.Description,
                Configuration = m.Configuration,
                Device = m.Device,
                Plugins = m.Plugins,
                CurrentHealthCheck = m.CurrentHealthCheck,
                CheckInterval = m.CheckInterval,
                CreatedAt = m.CreatedAt,
                IsAcknowledged = m.IsAcknowledged,
                IsMonitored = m.IsMonitored,
                MonitorMetrics = _dbCtx.SystemMetrics
                    .Where(metric => metric.SystemMonitorId == m.SystemMonitorId)
                    .OrderByDescending(metric => metric.Timestamp)
                    .Take(5)
                    .Select(metric => new OverviewMetric
                    {
                       Timestamp = metric.Timestamp,
                       Metric = metric.CPUUsage,
                    })
                    .ToList()
            })
            .ToListAsync();
        return Ok(results);
    }

    [MapToApiVersion(1)]
    [HttpGet("{SystemMonitorId:guid}")]
    public async Task<ActionResult<SystemMonitor>> GetMonitorById(Guid SystemMonitorId)
    {
        try
        {
            var monitor = await _dbCtx.SystemMonitors
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.SystemMonitorId == SystemMonitorId);

            if (monitor == null)
                return NotFound();

            if (monitor.Plugins.Count == 0) return Ok(monitor);

            var pluginIds = monitor.Plugins.Select(p => p.ToString()).ToList();

            var plugins = await _dbCtx.MonitorPlugins
                .Where(p => pluginIds.Contains(p.PluginId)) // pluginIds is List<string>
                .AsNoTracking()
                .ToListAsync();

            monitor.PluginDetails = plugins;

            return Ok(monitor);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching system monitor with plugins (ID: {Id})", SystemMonitorId);
            return StatusCode(500, "Internal server error");
        }
    }

    [MapToApiVersion(1)]
    [HttpPost]
    public async Task<ActionResult<SystemMonitor>> Create(SystemMonitor input)
    {
        var entity = new SystemMonitor
        {
            SystemMonitorId = Guid.NewGuid(),
            ServiceName = input.ServiceName,
            IPAddress = input.IPAddress,
            Description = input.Description,
            Port = input.Port,
            IsMonitored = true,
            Device = input.Device,
            FailureCount = 0,
            Plugins = input.Plugins,
            Configuration = input.Configuration,
            CheckInterval = input.CheckInterval,
            IsAcknowledged = input.IsAcknowledged,
            SnoozeUntil = input.SnoozeUntil,
            CurrentHealthCheck = MonitoringStatus.UnknownStatus,
        };

        _dbCtx.SystemMonitors.Add(entity);
        await _dbCtx.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMonitorById), new { SystemMonitorId = entity.SystemMonitorId }, entity);
    }

    [MapToApiVersion(1)]
    [HttpPut("Plugin/{SystemMonitorId:guid}")]
    public async Task<IActionResult> UpdateServicePluginConfiguration([FromRoute] Guid SystemMonitorId,
        [FromQuery] string PluginId, [FromBody] object Configuration)
    {
        var monitor = await _dbCtx.SystemMonitors
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.SystemMonitorId == SystemMonitorId);

        if (monitor == null)
            return NotFound();

        if (monitor.Plugins.Count == 0) return Ok(monitor);

        var stringifyConfig = JsonSerializer.Serialize(Configuration);

        if (string.IsNullOrWhiteSpace(stringifyConfig)) return BadRequest();
        try
        {
            JsonDocument.Parse(stringifyConfig);
            monitor.Configuration = JsonSerializer.Serialize(Configuration);

            _dbCtx.SystemMonitors.Update(monitor);
            await _dbCtx.SaveChangesAsync();
            return Ok(CreatedAtAction(nameof(GetMonitorById), new { id = SystemMonitorId }));
        }
        catch
        {
            return BadRequest();
        }
    }

    [MapToApiVersion(1)]
    [HttpPut("{SystemMonitorId:guid}")]
    public async Task<IActionResult> Update(Guid SystemMonitorId, SystemMonitor updated)
    {
        var existing = await _dbCtx.SystemMonitors.FindAsync(SystemMonitorId);
        if (existing is null) return NotFound();

        updated.SystemMonitorId = existing.SystemMonitorId;
        updated.CreatedAt = existing.CreatedAt;
        updated.Configuration = existing.Configuration;

        _dbCtx.Entry(existing).CurrentValues.SetValues(updated);
        await _dbCtx.SaveChangesAsync();

        return NoContent();
    }

    [MapToApiVersion(1)]
    [HttpDelete("{MonitorId:guid}")]
    public async Task<IActionResult> Delete(Guid MonitorId)
    {
        var monitor = await _dbCtx.SystemMonitors.FindAsync(MonitorId);
        if (monitor is null) return NotFound();

        _dbCtx.SystemMonitors.Remove(monitor);
        await _dbCtx.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpGet("Agent/Releases")]
        public async Task<IActionResult> GetAgentReleaseAsync()
        {
          var result= await _githubService.GetLatestReleaseAsync(_agentConfig.Value.GitUsername, _agentConfig.Value.GitRepository);
          
          return Ok(result);
        }
        
        [HttpPost("Deploy")]
        // [Authorize]
        public async Task<IActionResult> InitiateAgentDeployment([FromBody] AgentDeploymentRequest request, CancellationToken cancellationToken)
        {
            // logger.LogInformation($"Deployment requested by {User.Identity!.Name} to {request.Environment}");
            // await agentContractor.CreateNewAgentAsync(request, cancellationToken);

            var agentRemoteConfig = agentContractor.CreateAgentConfiguration(new AgentSettings
            {
                AgentVersion = request.AgentVersion,
                AgentAPIPort = int.Parse(_agentConfig.Value.DefaultPort),
                APIBaseUrl = _agentConfig.Value.AgentRepositoryPath,
                AgentID = ""
            });
        
            // Start the deployment process
            var deployment = await _ansibleDeploymentService.RunAnsiblePlaybookAsync(request, agentRemoteConfig);
        
            return Ok(deployment);
        }
}