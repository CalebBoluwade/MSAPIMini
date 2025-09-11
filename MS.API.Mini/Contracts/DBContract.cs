using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;

namespace MS.API.Mini.Contracts;

public interface IDBContract
{
    Task<List<SystemMonitor>> GetAllEntitiesByType(string EntityTYPE);

    Task<List<SystemMetric>> GetSystemMetricsAsync(string AgentID, string Entity, long startPeriod, long endPeriod);

    public Task<List<DiskData>>
        GetSystemDiskData(string AgentID);

    Task<List<SystemMetric>> GetMetricsAsync(
        string metric,
        DateTime startDate,
        DateTime endDate,
        List<string> agents);
}

public class DBContractor(ILogger<DBContractor> logger, MonitorDBContext ctx): IDBContract
{
    public async Task<List<SystemMonitor>> GetAllEntitiesByType(string Entity){
        if (string.IsNullOrEmpty(Entity)) return [];
        
        // var t = from entity in ctx.ApplicationEntities
        //         join mEntity in ctx.SyntheticMonitorData
        //         on entity.
            
        var EntityList = await ctx.SystemMonitors.Where(x => x.Device == Entity).Select(y => new SystemMonitor
        {
            Device = y.Device,
            IPAddress = y.IPAddress,
            Agent = y.Agent,
        }).ToListAsync();
        
        return EntityList;
    }

    public async Task<List<SystemMetric>> GetSystemMetricsAsync(string AgentID, string Entity, long startPeriod, long endPeriod)
    {
        return await ctx.SystemMetrics
            .Where(x => x.AgentID == AgentID && 
                        x.Timestamp >= startPeriod && 
                        x.Timestamp <= endPeriod)
            .OrderByDescending(x => x.Timestamp)
            .Take(200)
            .ToListAsync();
    }

    public async Task<List<DiskData>> GetSystemDiskData(string AgentID)
    {
        return await ctx.SystemDiskData
            .Where(x => x.AgentID == AgentID)
            .ToListAsync();
    }
    
    public async Task<List<SystemMetric>> GetMetricsAsync(
        string metric,
        DateTime startDate,
        DateTime endDate,
        List<string> agents)
    {
        var query = ctx.SystemMetrics;
            // .Include(m => m.AgentId)
            // .Where(m => agents.Contains(m.AgentId))
            // .Where(m => m.Timestamp >= startDate &&
            //             m.Timestamp <= endDate);

        // if (agents.Count > 0)
        // {
        //     query = query.Where(m => agents.Contains(m.AgentID));
        // }
        
        var dbMetrics = await query.OrderBy(m => m.Timestamp).ToListAsync();

        var AgentData = await ctx.Agents.ToListAsync();
        
        if (AgentData.Count == 0)
        {
            logger.LogWarning("No Redis data found for agents.");
            return dbMetrics; // Return only DB metrics if Redis data is missing
        }
        
        // Step 3: Filter Redis data by the provided agent list
        // var filteredRedisData = AgentData.Where(r => agents.Contains(r.AGENT_ID)).ToList();
        // var filteredRedisData = AgentData.Where(r => agents.Contains(r.AGENT_ID)).ToList();

        // Step 4: Join Redis data with the database metrics
        var joinedResults = dbMetrics
            .Select(dbMetric =>
            {
                var Agent = AgentData.FirstOrDefault(r => r.AgentID == dbMetric.AgentID);
                return new SystemMetric
                {
                    Timestamp = dbMetric.Timestamp,
                    TimestampMem = dbMetric.TimestampMem,
                    AgentID = dbMetric.AgentID,
                    CPUUsage = dbMetric.CPUUsage,
                    MemoryUsage = dbMetric.MemoryUsage,
                    AgentName = Agent?.AgentHostName!,
                    AgentVersion = Agent?.AgentHostName!,
                    AgentOS = Agent!.AgentID!
                };
            })
            .ToList();

        // logger.LogDebug("Joined data: {@JoinedResults}", joinedResults);

        return joinedResults;
    }
}