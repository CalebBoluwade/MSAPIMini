using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MS.API.Mini.Migrations
{
    /// <inheritdoc />
    public partial class AddedParams_ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PluginMetrics",
                table: "PluginMonitoringResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MainStatus",
                table: "MonitoringResultHistory",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PluginMetrics",
                table: "PluginMonitoringResults");

            migrationBuilder.DropColumn(
                name: "MainStatus",
                table: "MonitoringResultHistory");
        }
    }
}
