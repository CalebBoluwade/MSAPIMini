using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MS.API.Mini.Migrations
{
    /// <inheritdoc />
    public partial class AddedCols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SystemMonitor",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCheckTime",
                table: "SystemMonitor",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastServiceUpTime",
                table: "SystemMonitor",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "NetworkDeviceMetricData",
                columns: table => new
                {
                    SystemMonitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceName = table.Column<string>(type: "text", nullable: false),
                    DeviceIP = table.Column<string>(type: "text", nullable: false),
                    MetricName = table.Column<string>(type: "text", nullable: false),
                    MetricDescription = table.Column<string>(type: "text", nullable: false),
                    MetricValue = table.Column<string>(type: "text", nullable: false),
                    LastPoll = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "SystemDiskData",
                columns: table => new
                {
                    AgentID = table.Column<string>(type: "character varying(55)", maxLength: 55, nullable: false),
                    Drive = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: false),
                    FormatFree = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FormatSize = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Free = table.Column<long>(type: "bigint", nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: true),
                    Used = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_SystemDiskData_Agents_AgentID",
                        column: x => x.AgentID,
                        principalTable: "Agents",
                        principalColumn: "AgentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemMetricData",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<long>(type: "bigint", nullable: true),
                    AgentID = table.Column<string>(type: "character varying(25)", nullable: false),
                    TimestampMem = table.Column<long>(type: "bigint", nullable: true),
                    CPUUsage = table.Column<double>(type: "double precision", nullable: false),
                    MemoryUsage = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemMetricData", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SystemMetricData_Agents_AgentID",
                        column: x => x.AgentID,
                        principalTable: "Agents",
                        principalColumn: "AgentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemDiskData_AgentID",
                table: "SystemDiskData",
                column: "AgentID");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetricData_AgentID",
                table: "SystemMetricData",
                column: "AgentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NetworkDeviceMetricData");

            migrationBuilder.DropTable(
                name: "SystemDiskData");

            migrationBuilder.DropTable(
                name: "SystemMetricData");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SystemMonitor");

            migrationBuilder.DropColumn(
                name: "LastCheckTime",
                table: "SystemMonitor");

            migrationBuilder.DropColumn(
                name: "LastServiceUpTime",
                table: "SystemMonitor");
        }
    }
}
