using MS.API.Mini.Data;
using MS.API.Mini.Extensions;

namespace MS.API.Mini.Extensions;

public static class DatabaseConfiguration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var ConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING");

        if (string.IsNullOrEmpty(ConnectionString))
        {
            throw new Exceptions.ConfigurationMissingException("MYSQL_CONNECTION_STRING environment variable is empty");
        }
        
        services.AddDbContext<MonitorDBContext>(options =>
            options.UseNpgsql(ConnectionString));
    }
}