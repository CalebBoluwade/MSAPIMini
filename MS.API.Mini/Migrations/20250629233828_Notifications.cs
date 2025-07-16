using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MS.API.Mini.Migrations
{
    /// <inheritdoc />
    public partial class Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPlatforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Configuration = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPlatforms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: false),
                    Password = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    WorkEmail = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    MFASecret = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsMFAEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupNotificationPlatforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    PlatformId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupNotificationPlatforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupNotificationPlatforms_NotificationGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "NotificationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupNotificationPlatforms_NotificationPlatforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "NotificationPlatforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceNotificationGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SystemMonitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    DBUserId = table.Column<long>(type: "bigint", nullable: true),
                    NotificationGroupId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceNotificationGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceNotificationGroups_NotificationGroups_NotificationGr~",
                        column: x => x.NotificationGroupId,
                        principalTable: "NotificationGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceNotificationGroups_SystemMonitor_SystemMonitorId",
                        column: x => x.SystemMonitorId,
                        principalTable: "SystemMonitor",
                        principalColumn: "SystemMonitorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceNotificationGroups_Users_DBUserId",
                        column: x => x.DBUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotificationGroups_NotificationGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "NotificationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNotificationGroups_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotificationPlatforms_GroupId",
                table: "GroupNotificationPlatforms",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupNotificationPlatforms_PlatformId",
                table: "GroupNotificationPlatforms",
                column: "PlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceNotificationGroups_DBUserId",
                table: "ServiceNotificationGroups",
                column: "DBUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceNotificationGroups_NotificationGroupId",
                table: "ServiceNotificationGroups",
                column: "NotificationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceNotificationGroups_SystemMonitorId",
                table: "ServiceNotificationGroups",
                column: "SystemMonitorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationGroups_GroupId",
                table: "UserNotificationGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationGroups_UserId",
                table: "UserNotificationGroups",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupNotificationPlatforms");

            migrationBuilder.DropTable(
                name: "ServiceNotificationGroups");

            migrationBuilder.DropTable(
                name: "UserNotificationGroups");

            migrationBuilder.DropTable(
                name: "NotificationPlatforms");

            migrationBuilder.DropTable(
                name: "NotificationGroups");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
