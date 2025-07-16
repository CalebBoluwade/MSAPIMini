using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Models;

namespace MS.API.Mini.Data;
using Microsoft.EntityFrameworkCore;

public class MonitorDBContext(DbContextOptions<MonitorDBContext> options) : DbContext(options)
{
    public DbSet<SystemMonitor> SystemMonitors { get; set; }
    public DbSet<MonitorPlugin> MonitorPlugins { get; set; }
    
    public DbSet<MonitoringResultHistory> MonitoringResultHistory { get; set; }
    public DbSet<PluginMonitoringResult> PluginResults { get; set; }
    
    public DbSet<NetworkDeviceMetric> NetworkDeviceMetrics { get; set; }
    
    public DbSet<SystemMetric> SystemMetrics { get; set; }
    
    public DbSet<DiskData> SystemDiskData { get; set; }
        
    public DbSet<Agents> Agents { get; set; }
    
    public DbSet<NotificationGroup> NotificationGroups { get; set; }
    public DbSet<NotificationPlatforms> NotificationPlatforms { get; set; }
    public DbSet<UserNotificationGroup> UserNotificationGroups { get; set; }
    public DbSet<ServiceNotificationGroup> ServiceNotificationGroups { get; set; }
    public DbSet<GroupNotificationPlatforms> GroupNotificationPlatforms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SystemMonitor>()
            .Property(a => a.CurrentHealthCheck)
            .HasDefaultValue(MonitoringStatus.UnknownStatus)
            .HasConversion(
                v => v.ToString(),
                v => (MonitoringStatus)Enum.Parse(typeof(MonitoringStatus), v));
        
        modelBuilder.Entity<MonitorPlugin>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            
            entity.HasIndex(e => e.PluginType);

            PluginType parsed;
            entity.Property(e => e.PluginType)
                .HasDefaultValue(PluginType.HealthCheck)
                .HasConversion(
                    v => v.ToString(), // enum -> string
                    v => Enum.TryParse<PluginType>(v, true, out parsed)
                        ? parsed
                        : PluginType.Unknown);
        });

        modelBuilder.Entity<SystemMonitor>(entity =>
        {
            entity.HasIndex(e => e.ServiceName).IsUnique();
            
            entity.Property(e => e.SystemMonitorId).HasDefaultValueSql("uuid_generate_v4()");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.RetryCount)
                .HasDefaultValueSql("3");

            entity.Property(e => e.Configuration)
                .HasDefaultValueSql("'{}'");
            
            entity.Property(e => e.CurrentHealthCheck)
                .HasDefaultValue(MonitoringStatus.UnknownStatus)
                .HasConversion(
                    v => v.ToString(),
                    v => (MonitoringStatus)Enum.Parse(typeof(MonitoringStatus), v));

            entity.Property(e => e.CheckInterval)
                .HasDefaultValueSql("'*/15 * * * *'");
            
            entity.Property(e => e.Plugins)
                .HasColumnType("text[]")
                .HasDefaultValueSql("ARRAY[]::text[]");
        });
        
        modelBuilder.Entity<MonitoringResultHistory>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.MainStatus).IsRequired();
            entity.Property(e => e.ExecutionTime).HasDefaultValueSql("NOW()");

            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (MonitoringStatus)Enum.Parse(typeof(MonitoringStatus), v));
            
            entity.Property(e => e.MainStatus)
                .HasConversion(
                    v => v.ToString(),
                    v => (MonitoringStatus)Enum.Parse(typeof(MonitoringStatus), v));

            entity.HasMany(e => e.PluginMonitoringResults)
                .WithOne(p => p.MonitoringResult)
                .HasForeignKey(p => p.MonitoringResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<PluginMonitoringResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Status).IsRequired();
            
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (MonitoringStatus)Enum.Parse(typeof(MonitoringStatus), v));
        });
        
        modelBuilder.Entity<Agents>()
            .HasKey(a => a.AgentID);
        
        modelBuilder.Entity<Agents>(entity =>
        {
            entity.Property(e => e.DateAdded)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        
        modelBuilder.Entity<Agents>()
            .HasIndex(ag => new { ag.AgentID, ag.AgentHostAddress })
            .IsUnique();

        modelBuilder.Entity<ServiceNotificationGroup>()
            .HasOne(g => g.Service);
    }
}