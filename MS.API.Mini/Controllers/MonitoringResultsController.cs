using System.Reflection;
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
        var pageNumber = 1; // example: first page
        var pageSize = 20;  // services per page
        
        var results = await _dbCtx.MonitoringResultHistory
            .Include(r => r.Service)
            .Select(x => new TrackerRecordDTO
            {
                ServiceName = x.Service.ServiceName,
                IPAddress = x.Service.IPAddress,
                CurrentHealthCheck = x.Status
            })
            // .Take(50)
            .GroupBy(x => x.ServiceName)
            // .OrderBy(g => g.Key) // ensures consistent ordering
            // .Take(pageSize)
            .ToListAsync();

        // var results = await _dbCtx.SystemMonitors
        //     .OrderBy(s => s.ServiceName) // keep ordering consistent
        //     .Skip((pageNumber - 1) * pageSize)
        //     .Take(pageSize)
        //     .Select(s => new TrackerRecordDTO
        //     {
        //         ServiceName = s.ServiceName,
        //         IPAddress = s.IPAddress,
        //         CurrentHealthCheck = _dbCtx.MonitoringResultHistory
        //             .Where(m => m.SystemMonitorId == s.SystemMonitorId)
        //             .OrderByDescending(m => m.ExecutionTime) // use your timestamp/PK
        //             .Select(m => m.Status)
        //             .FirstOrDefault()
        //     })
        //     .ToListAsync();

        
        return Ok(results);
    }
    
    [MapToApiVersion(1)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMonitoringResult(Guid id, [FromQuery] long StartDate, [FromQuery] long EndDate)
    {
        try
        {
            var pluginTypes = await _dbCtx.MonitorPlugins
                .ToDictionaryAsync(p => p.PluginId, p => p.PluginType);

            // Load monitoring results and related plugin results
            var results = await _dbCtx.MonitoringResultHistory
                .Where(r => r.SystemMonitorId == id)
                .Include(r => r.PluginMonitoringResults)
                .ThenInclude(pluginMonitoringResult => pluginMonitoringResult.PluginMetrics)
                .AsSplitQuery()
                .ToListAsync();

            var dtoResults = results.Select(r => new MonitoringResultDTO
            {
                Id = r.Id,
                SystemMonitorId = r.SystemMonitorId,
                Status = r.Status.ToString(),
                MainStatus = r.MainStatus.ToString(),
                CheckedAt = r.ExecutionTime,
                Message = r.HealthReport,
                PluginResults = r.PluginMonitoringResults.OrderByDescending(x => x.HealthReport).Select(p => new PluginResultDTO
                {
                    PluginName = p.PluginName,
                    PluginDescription = p.PluginDescription,
                    PluginType = pluginTypes.TryGetValue(p.PluginName, out var pluginType)
                        ? pluginType
                        : PluginType.Unknown,
                    // PluginMetrics = CreateSafePluginMetrics(p.PluginMetrics),
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
    
    // Helper method to safely handle PluginMetrics
    private object CreateSafePluginMetrics(object pluginMetrics)
    {
        if (pluginMetrics == null)
            return null;

        try
        {
            // Option 1: Convert to a simple dictionary or anonymous object
            // This breaks any potential circular references
            var type = pluginMetrics.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                .ToDictionary(
                    prop => prop.Name,
                    prop => prop.GetValue(pluginMetrics)
                );
        
            return properties;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to serialize PluginMetrics safely");
            return new { error = "Metrics unavailable" };
        }
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive 
               || type.IsEnum 
               || type == typeof(string) 
               || type == typeof(decimal) 
               || type == typeof(DateTime) 
               || type == typeof(DateTimeOffset) 
               || type == typeof(TimeSpan) 
               || type == typeof(Guid)
               || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSimpleType(type.GetGenericArguments()[0]));
    }
}