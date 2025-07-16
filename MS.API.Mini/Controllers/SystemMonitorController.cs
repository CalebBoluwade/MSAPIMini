using System.Text.Json;
using Asp.Versioning;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;

namespace MS.API.Mini.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemMonitorController(MonitorDBContext _dbCtx, ILogger<SystemMonitorController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SystemMonitor>>> GetSystemMonitors()
    {
        var results = await _dbCtx.SystemMonitors.OrderBy(x => x.CreatedAt).ToListAsync();
        return Ok(results);
    }

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
            RetryCount = input.RetryCount,
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