using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace GrillBot.Database.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class EmoteGuildIdStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emotes_Users_UserId",
                table: "Emotes");

            migrationBuilder.DropIndex(
                name: "IX_Emotes_UserId",
                table: "Emotes");

            migrationBuilder.AddColumn<string>(
                name: "GuildId",
                table: "Emotes",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Emotes_GuildId_UserId",
                table: "Emotes",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Emotes_Guilds_GuildId",
                table: "Emotes",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Emotes_GuildUsers_GuildId_UserId",
                table: "Emotes",
                columns: new[] { "GuildId", "UserId" },
                principalTable: "GuildUsers",
                principalColumns: new[] { "GuildId", "UserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emotes_Guilds_GuildId",
                table: "Emotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Emotes_GuildUsers_GuildId_UserId",
                table: "Emotes");

            migrationBuilder.DropIndex(
                name: "IX_Emotes_GuildId_UserId",
                table: "Emotes");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Emotes");

            migrationBuilder.CreateIndex(
                name: "IX_Emotes_UserId",
                table: "Emotes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Emotes_Users_UserId",
                table: "Emotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
