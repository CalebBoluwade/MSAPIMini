using System.Text.Json;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Extensions;

namespace MS.API.Mini.Controllers;

public class SystemMonitorController(MonitorDBContext _dbCtx,
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
        return CreatedAtAction(nameof(GetMonitorById), new { entity.SystemMonitorId }, entity);
    }

    [MapToApiVersion(1)]
    [HttpPut("PluginConfigEdit/{SystemMonitorId:guid}")]
    public async Task<IActionResult> UpdateServicePluginConfiguration([FromRoute] Guid SystemMonitorId,
        [FromBody] object Configuration)
    {
        var monitor = await _dbCtx.SystemMonitors
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.SystemMonitorId == SystemMonitorId);

        if (monitor == null)
            return NotFound();

        if (monitor.Plugins.Count == 0) return BadRequest(monitor);

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
        updated.PollerNode = existing.PollerNode;
        // updated.Configuration = existing.Configuration;
        updated.Agent = existing.Agent;
        
        logger.LogInformation("Updating {@SystemMonitor}", updated);

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
    
}