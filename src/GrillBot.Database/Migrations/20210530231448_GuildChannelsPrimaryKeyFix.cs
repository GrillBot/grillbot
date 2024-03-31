using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class GuildChannelsPrimaryKeyFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Channels_GuildId_ChannelId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Users_UserId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchItems_Channels_GuildId_ChannelId",
                table: "SearchItems");

            migrationBuilder.DropIndex(
                name: "IX_SearchItems_GuildId_ChannelId",
                table: "SearchItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Channels",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_GuildId_ChannelId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Channels",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Channels",
                table: "Channels",
                columns: new[] { "GuildId", "Id", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchItems_GuildId_ChannelId_UserId",
                table: "SearchItems",
                columns: new[] { "GuildId", "ChannelId", "UserId" });

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Channels_GuildId_ChannelId_ProcessedUserId",
                table: "AuditLogs");

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
                name: "IX_AuditLogs_GuildId_ChannelId_ProcessedUserId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Channels",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Channels",
                table: "Channels",
                columns: new[] { "GuildId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchItems_GuildId_ChannelId",
                table: "SearchItems",
                columns: new[] { "GuildId", "ChannelId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_GuildId_ChannelId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ChannelId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Channels_GuildId_ChannelId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ChannelId" },
                principalTable: "Channels",
                principalColumns: new[] { "GuildId", "Id" },
                onDelete: ReferentialAction.Restrict);

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
    }
}
