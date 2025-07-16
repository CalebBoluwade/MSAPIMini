using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MS.API.Mini.Migrations
{
    /// <inheritdoc />
    public partial class Agents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SystemMonitorId",
                table: "SystemMonitor",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuid_generate_v4()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    AgentID = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    AgentHostName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AgentHostAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AgentPort = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    OS = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    AgentVersion = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    SDKVersion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LastSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AgentLicenseKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AgentLicenseKeyExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    VP = table.Column<bool>(type: "boolean", nullable: false),
                    AppOwnerID = table.Column<Guid>(type: "uuid", nullable: false),
                    AGENT_STATE = table.Column<string>(type: "text", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.AgentID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_AgentID_AgentHostAddress",
                table: "Agents",
                columns: new[] { "AgentID", "AgentHostAddress" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.AlterColumn<Guid>(
                name: "SystemMonitorId",
                table: "SystemMonitor",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuid_generate_v4()");
        }
    }
}
