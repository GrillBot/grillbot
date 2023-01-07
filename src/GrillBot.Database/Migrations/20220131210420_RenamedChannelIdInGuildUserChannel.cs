using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace GrillBot.Database.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class RenamedChannelIdInGuildUserChannel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserChannels_Channels_GuildId_Id",
                table: "UserChannels");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserChannels",
                newName: "ChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserChannels_Channels_GuildId_ChannelId",
                table: "UserChannels",
                columns: new[] { "GuildId", "ChannelId" },
                principalTable: "Channels",
                principalColumns: new[] { "GuildId", "ChannelId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserChannels_Channels_GuildId_ChannelId",
                table: "UserChannels");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "UserChannels",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserChannels_Channels_GuildId_Id",
                table: "UserChannels",
                columns: new[] { "GuildId", "Id" },
                principalTable: "Channels",
                principalColumns: new[] { "GuildId", "ChannelId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
