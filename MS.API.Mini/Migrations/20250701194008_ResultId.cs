using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MS.API.Mini.Migrations
{
    /// <inheritdoc />
    public partial class ResultId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PluginMonitoringResults",
                table: "PluginMonitoringResults");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PluginMonitoringResults");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PluginMonitoringResults",
                table: "PluginMonitoringResults",
                column: "MonitoringResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PluginMonitoringResults",
                table: "PluginMonitoringResults");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "PluginMonitoringResults",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PluginMonitoringResults",
                table: "PluginMonitoringResults",
                column: "Id");
        }
    }
}
