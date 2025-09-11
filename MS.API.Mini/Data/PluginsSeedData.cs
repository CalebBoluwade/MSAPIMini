using MS.API.Mini.Data.Models;

namespace MS.API.Mini.Data;

public static class DatabaseInitializer
{
    public static async Task SeedPlugins(MonitorDBContext context)
    {
        // Check if plugins already exist
        if (await context.MonitorPlugins.AnyAsync())
        {
            return; // Database has been seeded already
        }

        var allPlugins = new List<MonitorPlugin>
        {
            new()
            {
                Id = 1,
                PluginId = "HTTPMonitor",
                Name = "HTTP Check",
                PluginType = PluginType.HealthCheck,
                Description = "Checks HTTP endpoints for availability",
                CompatibleDeviceTypes = ["Web Modules", "Server"],
                ComingSoon = false
            },
            new()
            {
                Id = 2,
                PluginId = "AgentMonitor",
                Name = "Agent Health",
                PluginType = PluginType.Agent,
                Description = "Checks agent service health",
                CompatibleDeviceTypes = ["Server"],
                ComingSoon = true
            },
            new()
            {
                Id = 3,
                PluginId = "SSLChecker",
                Name = "SSL Check",
                PluginType = PluginType.Security,
                Description = "Validates SSL certificates",
                CompatibleDeviceTypes = ["Web Modules", "Server"],
                ComingSoon = false
            },
            new()
            {
                Id = 4,
                PluginId = "DatabaseMonitor",
                Name = "Database Gauge",
                PluginType = PluginType.HealthCheck,
                Description = "Specific Database Checks",
                CompatibleDeviceTypes = ["Database"],
                ComingSoon = false
            },
            new()
            {
                Id = 5,
                PluginId = "NetworkMonitor",
                Name = "Network Gauge",
                PluginType = PluginType.HealthCheck,
                Description = "Specific Network Checks using SNMP Protocol",
                CompatibleDeviceTypes = ["Network"],
                ComingSoon = false
            }
        };

        await context.MonitorPlugins.AddRangeAsync(allPlugins);
        await context.SaveChangesAsync();
    }
}