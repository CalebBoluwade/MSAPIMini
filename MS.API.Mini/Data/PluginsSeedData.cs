using MS.API.Mini.Data.Models;
using MS.API.Mini.Models;

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
                PluginId = "http_check",
                Name = "HTTP Check",
                PluginType = PluginType.HealthCheck,
                Description = "Checks HTTP endpoints for availability",
                CompatibleDeviceTypes = ["Web Modules", "Server"],
                ComingSoon = false
            },
            new()
            {
                Id = 2,
                PluginId = "ping_check",
                Name = "Ping Check",
                PluginType = PluginType.HealthCheck,
                Description = "Basic network ping check",
                CompatibleDeviceTypes = ["Network", "Server"],
                ComingSoon = false
            },
            new()
            {
                Id = 3,
                PluginId = "agent_health",
                Name = "Agent Health",
                PluginType = PluginType.Agent,
                Description = "Checks agent service health",
                CompatibleDeviceTypes = ["AGENT"],
                ComingSoon = true
            },
            new()
            {
                Id = 4,
                PluginId = "ssl_check",
                Name = "SSL Check",
                PluginType = PluginType.Security,
                Description = "Validates SSL certificates",
                CompatibleDeviceTypes = ["Web Modules", "Server"],
                ComingSoon = false
            }
        };

        await context.MonitorPlugins.AddRangeAsync(allPlugins);
        await context.SaveChangesAsync();
    }
}