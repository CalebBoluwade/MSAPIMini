using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MS.API.Mini.Migrations
{
    /// <inheritdoc />
    public partial class HealthCheck_Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CheckInterval",
                table: "SystemMonitor",
                type: "text",
                nullable: false,
                defaultValueSql: "'*/15 * * * *'",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "'*/5 * * * *'");

            migrationBuilder.AddColumn<int>(
                name: "CurrentHealthCheck",
                table: "SystemMonitor",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentHealthCheck",
                table: "SystemMonitor");

            migrationBuilder.AlterColumn<string>(
                name: "CheckInterval",
                table: "SystemMonitor",
                type: "text",
                nullable: false,
                defaultValueSql: "'*/5 * * * *'",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "'*/15 * * * *'");
        }
    }
}
