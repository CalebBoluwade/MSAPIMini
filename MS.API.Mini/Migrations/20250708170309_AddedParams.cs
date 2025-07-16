using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MS.API.Mini.Migrations
{
    /// <inheritdoc />
    public partial class AddedParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstanceName",
                table: "MonitoringResultHistory");

            migrationBuilder.RenameColumn(
                name: "ServicePluginId",
                table: "PluginMonitoringResults",
                newName: "PluginName");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentHealthCheck",
                table: "SystemMonitor",
                type: "text",
                nullable: false,
                defaultValue: "UnknownStatus",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PluginDescription",
                table: "PluginMonitoringResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PluginId",
                table: "PluginMonitoringResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PluginType",
                table: "MonitorPlugins",
                type: "text",
                nullable: false,
                defaultValue: "HealthCheck",
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PluginDescription",
                table: "PluginMonitoringResults");

            migrationBuilder.DropColumn(
                name: "PluginId",
                table: "PluginMonitoringResults");

            migrationBuilder.RenameColumn(
                name: "PluginName",
                table: "PluginMonitoringResults",
                newName: "ServicePluginId");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentHealthCheck",
                table: "SystemMonitor",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "UnknownStatus");

            migrationBuilder.AlterColumn<string>(
                name: "PluginType",
                table: "MonitorPlugins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "HealthCheck");

            migrationBuilder.AddColumn<string>(
                name: "InstanceName",
                table: "MonitoringResultHistory",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
