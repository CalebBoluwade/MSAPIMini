using MS.API.Mini.Data.Models;

namespace MS.API.Mini.Data;

using Microsoft.EntityFrameworkCore;

public class MonitorDBContext(DbContextOptions<MonitorDBContext> options) : DbContext(options)
{
    public DbSet<SystemMonitor> SystemMonitors { get; set; }
    public DbSet<MonitorPlugin> MonitorPlugins { get; set; }
    public DbSet<MonitoringResultHistory> MonitoringResultHistory { get; set; }
    
    public DbSet<PluginMonitoringResult> PluginResults { get; set; }
    
    public DbSet<PluginMetric> PluginMetrics { get; set; }

    public DbSet<NetworkDeviceMetric> NetworkDeviceMetrics { get; set; }

    public DbSet<SystemMetric> SystemMetrics { get; set; }

    public DbSet<DiskData> SystemDiskData { get; set; }

    public DbSet<Agent> Agents { get; set; }

    public DbSet<AvailablePoller> AvailablePollers { get; set; }
    
    public DbSet<NotificationPlatforms> NotificationPlatforms { get; set; }
    
    public DbSet<DBUser> Users { get; set; }
    
    public DbSet<MonitoringRule> MonitoringRules { get; set; }

    public DbSet<RuleConflict> RuleConflicts { get; set; }

    public DbSet<RuleEvaluation> RuleEvaluations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
            entity.HasKey(sm => sm.SystemMonitorId);

            entity.HasIndex(e => e.ServiceName).IsUnique();

            entity.HasIndex(e => new { e.IPAddress, e.Port }).IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Configuration)
                .HasDefaultValueSql("'{}'");

            entity.Property(e => e.CurrentHealthCheck)
                .HasDefaultValue(MonitoringStatus.UnknownStatus)
                .HasConversion(
                    v => v.ToString(),
                    v => (MonitoringStatus)Enum.Parse(typeof(MonitoringStatus), v ?? "UnknownStatus"))
                .HasDefaultValue(MonitoringStatus.UnknownStatus)
                .IsRequired();

            entity.Property(e => e.CurrentHealthCheck).ValueGeneratedNever();

            entity.Property(e => e.CheckInterval)
                .HasDefaultValueSql("'*/15 * * * *'");

            entity.Property(e => e.Plugins)
                .HasColumnType("text[]")
                .HasDefaultValueSql("ARRAY[]::text[]");

            entity.HasMany(s => s.MonitorMetrics)
                .WithOne(m => m.SystemMonitor)
                .HasForeignKey(m => m.SystemMonitorId);
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
                    v => (MonitoringStatus)Enum.Parse(typeof(MonitoringStatus), v ?? "UnknownStatus"))
                .HasDefaultValue(MonitoringStatus.UnknownStatus)
                .IsRequired();

            entity.Property(e => e.MainStatus)
                .HasConversion(
                    v => v.ToString(),
                    v => (MonitoringStatus)Enum.Parse(typeof(MonitoringStatus), v ?? "UnknownStatus"))
                .HasDefaultValue(MonitoringStatus.UnknownStatus)
                .IsRequired();

            entity.HasMany(e => e.PluginMonitoringResults)
                .WithOne(p => p.MonitoringResult)
                .HasForeignKey(p => p.MonitoringResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<PluginMetric>(entity =>
        {
            entity.HasKey(e => new { e.PluginResultId, e.PluginMetricId }); // Composite key
    
            entity.HasOne(e => e.PluginMonitoringResult)
                .WithMany(r => r.PluginMetrics)
                .HasForeignKey(e => e.PluginResultId);
          
            entity.HasOne(e => e.PluginMonitoringResult)
                .WithMany()
                .HasForeignKey(e => e.PluginMetricId);
        });

        modelBuilder.Entity<PluginMonitoringResult>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status).IsRequired();
            
            entity.HasMany(e => e.PluginMetrics)  
                .WithOne(m => m.PluginMonitoringResult)
                .HasForeignKey(m => m.PluginResultId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (MonitoringStatus)Enum.Parse(typeof(MonitoringStatus), v));
        });

        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasIndex(a => a.AgentID).IsUnique();

            entity.Property(e => e.AgentHostAddress).IsRequired();

            entity.HasIndex(ag => new { ag.AgentID, ag.AgentHostAddress })
                .IsUnique();

            entity.Property(e => e.IsMonitored)
                .HasDefaultValue(false);

            entity.Property(e => e.DateAdded)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure the primary key for SystemMetric
        modelBuilder.Entity<SystemMetric>()
            .HasKey(sm => sm.ID);

        // Create indexes for faster lookups
        modelBuilder.Entity<SystemMetric>()
            .HasIndex(sm => new { sm.AgentID, sm.Timestamp })
            .HasDatabaseName("IDX_SystemMetrics_Agent");

        // Configure the foreign key relationship between SystemMetric and Agent
        modelBuilder.Entity<SystemMetric>()
            .HasOne(sm => sm.Agent)
            .WithMany(a => a.SystemMetrics)
            .HasForeignKey(sm => sm.AgentID)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure the primary key for Disk
        modelBuilder.Entity<DiskData>()
            .HasKey(d => d.AgentID);

        // Configure the foreign key relationship between Disk and Agent
        modelBuilder.Entity<DiskData>()
            .HasOne(d => d.Agent)
            .WithMany(a => a.Disks)
            .HasForeignKey(d => d.AgentID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DiskData>()
            .HasIndex(e => new { e.AgentID, e.Drive })
            .IsUnique();

        modelBuilder.Entity<DiskData>()
            .HasIndex(d => new { d.AgentID, d.Drive })
            .HasDatabaseName("IDX_Disks_Agent");

        modelBuilder.Entity<NetworkDeviceMetric>()
            .HasKey(ndm => new { ndm.SystemMonitorId, ndm.DeviceIP, ndm.MetricName });

        modelBuilder.Entity<AvailablePoller>(entity =>
        {
            entity.Property(e => e.IPAddress).IsRequired();
            entity.HasIndex(e => e.IPAddress).IsUnique();
        });

        modelBuilder.Entity<RuleEvaluation>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        });
    }
}