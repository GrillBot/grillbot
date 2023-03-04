using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class NormalizationOfChannels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Channels_GuildId_ChannelId_ProcessedUserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Guilds_GuildId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Users_UserId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchItems_Channels_GuildId_ChannelId_UserId",
                table: "SearchItems");

            migrationBuilder.DropIndex(
                name: "IX_SearchItems_GuildId_ChannelId_UserId",
                table: "SearchItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Channels",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_Channels_UserId",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_GuildId_ChannelId_ProcessedUserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "FirstMessageAt",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "LastMessageAt",
                table: "Channels");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Channels",
                newName: "ChannelId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Channels",
                table: "Channels",
                columns: new[] { "GuildId", "ChannelId" });

            migrationBuilder.CreateTable(
                name: "UserChannels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Count = table.Column<long>(type: "bigint", nullable: false),
                    FirstMessageAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChannels", x => new { x.GuildId, x.Id, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserChannels_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChannels_Channels_GuildId_Id",
                        columns: x => new { x.GuildId, x.Id },
                        principalTable: "Channels",
                        principalColumns: new[] { "GuildId", "ChannelId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChannels_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchItems_GuildId_ChannelId",
                table: "SearchItems",
                columns: new[] { "GuildId", "ChannelId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_GuildId_ChannelId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ChannelId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserChannels_UserId",
                table: "UserChannels",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Channels_GuildId_ChannelId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ChannelId" },
                principalTable: "Channels",
                principalColumns: new[] { "GuildId", "ChannelId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchItems_Channels_GuildId_ChannelId",
                table: "SearchItems",
                columns: new[] { "GuildId", "ChannelId" },
                principalTable: "Channels",
                principalColumns: new[] { "GuildId", "ChannelId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Channels_GuildId_ChannelId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchItems_Channels_GuildId_ChannelId",
                table: "SearchItems");

            migrationBuilder.DropTable(
                name: "UserChannels");

            migrationBuilder.DropIndex(
                name: "IX_SearchItems_GuildId_ChannelId",
                table: "SearchItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Channels",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_GuildId_ChannelId",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "Channels",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "Channels",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Count",
                table: "Channels",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstMessageAt",
                table: "Channels",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMessageAt",
                table: "Channels",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Channels",
                table: "Channels",
                columns: new[] { "GuildId", "Id", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchItems_GuildId_ChannelId_UserId",
                table: "SearchItems",
                columns: new[] { "GuildId", "ChannelId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Channels_UserId",
                table: "Channels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_GuildId_ChannelId_ProcessedUserId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ChannelId", "ProcessedUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Channels_GuildId_ChannelId_ProcessedUserId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ChannelId", "ProcessedUserId" },
                principalTable: "Channels",
                principalColumns: new[] { "GuildId", "Id", "UserId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Guilds_GuildId",
                table: "Channels",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Users_UserId",
                table: "Channels",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchItems_Channels_GuildId_ChannelId_UserId",
                table: "SearchItems",
                columns: new[] { "GuildId", "ChannelId", "UserId" },
                principalTable: "Channels",
                principalColumns: new[] { "GuildId", "Id", "UserId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
