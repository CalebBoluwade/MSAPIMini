using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MS.API.Mini.Migrations
{
    /// <inheritdoc />
    public partial class MonitoringResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CurrentHealthCheck",
                table: "SystemMonitor",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Used",
                table: "SystemDiskData",
                type: "numeric(18,6)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PluginType",
                table: "MonitorPlugins",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MonitoringResultHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SystemMonitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Status = table.Column<string>(type: "text", nullable: false),
                    HealthReport = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    InstanceName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringResultHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoringResultHistory_SystemMonitor_SystemMonitorId",
                        column: x => x.SystemMonitorId,
                        principalTable: "SystemMonitor",
                        principalColumn: "SystemMonitorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PluginMonitoringResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonitoringResultId = table.Column<long>(type: "bigint", nullable: false),
                    ServicePluginId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    HealthReport = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginMonitoringResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluginMonitoringResults_MonitoringResultHistory_MonitoringR~",
                        column: x => x.MonitoringResultId,
                        principalTable: "MonitoringResultHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemMonitor_ServiceName",
                table: "SystemMonitor",
                column: "ServiceName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonitorPlugins_Name",
                table: "MonitorPlugins",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonitorPlugins_PluginType",
                table: "MonitorPlugins",
                column: "PluginType");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringResultHistory_SystemMonitorId",
                table: "MonitoringResultHistory",
                column: "SystemMonitorId");

            migrationBuilder.CreateIndex(
                name: "IX_PluginMonitoringResults_MonitoringResultId",
                table: "PluginMonitoringResults",
                column: "MonitoringResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PluginMonitoringResults");

            migrationBuilder.DropTable(
                name: "MonitoringResultHistory");

            migrationBuilder.DropIndex(
                name: "IX_SystemMonitor_ServiceName",
                table: "SystemMonitor");

            migrationBuilder.DropIndex(
                name: "IX_MonitorPlugins_Name",
                table: "MonitorPlugins");

            migrationBuilder.DropIndex(
                name: "IX_MonitorPlugins_PluginType",
                table: "MonitorPlugins");

            migrationBuilder.DropColumn(
                name: "PluginType",
                table: "MonitorPlugins");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentHealthCheck",
                table: "SystemMonitor",
                type: "integer",
                nullable: false,
                defaultValue: 4,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "Used",
                table: "SystemDiskData",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldNullable: true);
        }
    }
}
