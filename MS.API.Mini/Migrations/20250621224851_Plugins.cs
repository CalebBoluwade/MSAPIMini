using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MS.API.Mini.Migrations
{
    /// <inheritdoc />
    public partial class Plugins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Plugins",
                table: "SystemMonitor",
                type: "text[]",
                nullable: false,
                defaultValue: Array.Empty<string>() // or new string[0]
            );
            
            migrationBuilder.AlterColumn<List<string>>(
                name: "Plugins",
                table: "SystemMonitor",
                type: "text[]",
                nullable: false,
                defaultValueSql: "ARRAY[]::text[]",
                oldClrType: typeof(List<string>),
                oldType: "text[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<string>>(
                name: "Plugins",
                table: "SystemMonitor",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldDefaultValueSql: "ARRAY[]::text[]");
        }
    }
}
