using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrillBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemovedOldAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogFiles");

            migrationBuilder.DropTable(
                name: "AuditLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ProcessedUserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ChannelId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    DiscordAuditLogItemId = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Channels_GuildId_ChannelId",
                        columns: x => new { x.GuildId, x.ChannelId },
                        principalTable: "Channels",
                        principalColumns: new[] { "GuildId", "ChannelId" });
                    table.ForeignKey(
                        name: "FK_AuditLogs_GuildUsers_GuildId_ProcessedUserId",
                        columns: x => new { x.GuildId, x.ProcessedUserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" });
                    table.ForeignKey(
                        name: "FK_AuditLogs_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ProcessedUserId",
                        column: x => x.ProcessedUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogFiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditLogItemId = table.Column<long>(type: "bigint", nullable: false),
                    Filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogFiles_AuditLogs_AuditLogItemId",
                        column: x => x.AuditLogItemId,
                        principalTable: "AuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogFiles_AuditLogItemId",
                table: "AuditLogFiles",
                column: "AuditLogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_GuildId_ChannelId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ChannelId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_GuildId_ProcessedUserId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ProcessedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ProcessedUserId",
                table: "AuditLogs",
                column: "ProcessedUserId");
        }
    }
}
