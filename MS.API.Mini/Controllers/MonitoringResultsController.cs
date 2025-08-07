using Asp.Versioning;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Extensions;

namespace MS.API.Mini.Controllers;

public class MonitorResultsController(MonitorDBContext _dbCtx, ILogger<MonitorResultsController> logger) : ControllerBaseExtension
{
    [MapToApiVersion(1)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await _dbCtx.MonitoringResultHistory
            .Include(r => r.Service)
            .Select(x => new TrackerRecordDTO
            {
                ServiceName = x.Service.ServiceName,
                IPAddress = x.Service.IPAddress,
                CurrentHealthCheck = x.Status
            }).GroupBy(x => x.ServiceName)
            .ToListAsync();
        
        return Ok(results);
    }
    
    [MapToApiVersion(1)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetResult(Guid id, [FromQuery] long StartDate, [FromQuery] long EndDate)
    {
        try
        {
            var pluginTypes = await _dbCtx.MonitorPlugins
                .ToDictionaryAsync(p => p.PluginId, p => p.PluginType);

            // Load monitoring results and related plugin results
            var results = await _dbCtx.MonitoringResultHistory
                .Where(r => r.SystemMonitorId == id)
                .Include(r => r.PluginMonitoringResults)
                .ToListAsync();

            var dtoResults = results.Select(r => new MonitoringResultDTO
            {
                Id = r.Id,
                SystemMonitorId = r.SystemMonitorId,
                Status = r.Status.ToString(),
                MainStatus = r.MainStatus.ToString(),
                CheckedAt = r.ExecutionTime,
                Message = r.HealthReport,
                PluginResults = r.PluginMonitoringResults.Select(p => new PluginResultDTO
                {
                    PluginName = p.PluginName,
                    PluginDescription = p.PluginDescription,
                    PluginType = pluginTypes.TryGetValue(p.PluginName, out var pluginType)
                        ? pluginType
                        : PluginType.Unknown,
                    PluginMetrics = p.PluginMetrics,
                    Status = p.Status.ToString(),
                    Output = p.HealthReport,
                    CheckedAt = r.ExecutionTime,
                }).ToList()
            }).ToList();

            // Always return 200 with a list (even if it's empty)
            return Ok(dtoResults);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to Get Monitor Results");
            return Ok();
        }
    }
}