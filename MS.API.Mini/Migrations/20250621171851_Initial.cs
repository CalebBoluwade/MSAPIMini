using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MS.API.Mini.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemMonitor",
                columns: table => new
                {
                    SystemMonitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IPAddress = table.Column<string>(type: "text", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    IsMonitored = table.Column<string>(type: "text", nullable: false),
                    Device = table.Column<string>(type: "text", nullable: false),
                    HealthReport = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "3"),
                    Configuration = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'{}'"),
                    CheckInterval = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'*/5 * * * *'"),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    SnoozeUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemMonitor", x => x.SystemMonitorId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemMonitor");
        }
    }
}
