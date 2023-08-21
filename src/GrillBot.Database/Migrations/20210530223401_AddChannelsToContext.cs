using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Database.Migrations
{
    public partial class AddChannelsToContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_GuildChannel_GuildId_ChannelId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildChannel_Guilds_GuildId",
                table: "GuildChannel");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildChannel_Users_UserId",
                table: "GuildChannel");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchItems_GuildChannel_GuildId_ChannelId",
                table: "SearchItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildChannel",
                table: "GuildChannel");

            migrationBuilder.RenameTable(
                name: "GuildChannel",
                newName: "Channels");

            migrationBuilder.RenameIndex(
                name: "IX_GuildChannel_UserId",
                newName: "IX_Channels_UserId",
                table: "Channels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Channels",
                table: "Channels",
                columns: new[] { "GuildId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Channels_GuildId_ChannelId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ChannelId" },
                principalTable: "Channels",
                principalColumns: new[] { "GuildId", "Id" },
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
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchItems_Channels_GuildId_ChannelId",
                table: "SearchItems",
                columns: new[] { "GuildId", "ChannelId" },
                principalTable: "Channels",
                principalColumns: new[] { "GuildId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Channels_GuildId_ChannelId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Guilds_GuildId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Users_UserId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchItems_Channels_GuildId_ChannelId",
                table: "SearchItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Channels",
                table: "Channels");

            migrationBuilder.RenameTable(
                name: "Channels",
                newName: "GuildChannel");

            migrationBuilder.RenameIndex(
                name: "IX_Channels_UserId",
                newName: "IX_GuildChannel_UserId",
                table: "GuildChannel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildChannel",
                table: "GuildChannel",
                columns: new[] { "GuildId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_GuildChannel_GuildId_ChannelId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ChannelId" },
                principalTable: "GuildChannel",
                principalColumns: new[] { "GuildId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildChannel_Guilds_GuildId",
                table: "GuildChannel",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildChannel_Users_UserId",
                table: "GuildChannel",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchItems_GuildChannel_GuildId_ChannelId",
                table: "SearchItems",
                columns: new[] { "GuildId", "ChannelId" },
                principalTable: "GuildChannel",
                principalColumns: new[] { "GuildId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
