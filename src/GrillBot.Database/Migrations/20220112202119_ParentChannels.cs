using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class ParentChannels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentChannelId",
                table: "Channels",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Channels_GuildId_ParentChannelId",
                table: "Channels",
                columns: new[] { "GuildId", "ParentChannelId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Channels_GuildId_ParentChannelId",
                table: "Channels",
                columns: new[] { "GuildId", "ParentChannelId" },
                principalTable: "Channels",
                principalColumns: new[] { "GuildId", "ChannelId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Channels_GuildId_ParentChannelId",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_Channels_GuildId_ParentChannelId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "ParentChannelId",
                table: "Channels");
        }
    }
}
