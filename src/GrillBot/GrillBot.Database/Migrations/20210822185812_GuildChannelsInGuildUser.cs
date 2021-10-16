using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Database.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class GuildChannelsInGuildUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserChannels_GuildId_UserId",
                table: "UserChannels",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserChannels_GuildUsers_GuildId_UserId",
                table: "UserChannels",
                columns: new[] { "GuildId", "UserId" },
                principalTable: "GuildUsers",
                principalColumns: new[] { "GuildId", "UserId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserChannels_GuildUsers_GuildId_UserId",
                table: "UserChannels");

            migrationBuilder.DropIndex(
                name: "IX_UserChannels_GuildId_UserId",
                table: "UserChannels");
        }
    }
}
