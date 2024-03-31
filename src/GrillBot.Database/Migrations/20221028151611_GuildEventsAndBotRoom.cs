using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class GuildEventsAndBotRoom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildEvent");

            migrationBuilder.AddColumn<string>(
                name: "BotRoomChannelId",
                table: "Guilds",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmoteSuggestionsFrom",
                table: "Guilds",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmoteSuggestionsTo",
                table: "Guilds",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BotRoomChannelId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "EmoteSuggestionsFrom",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "EmoteSuggestionsTo",
                table: "Guilds");

            migrationBuilder.CreateTable(
                name: "GuildEvent",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    From = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    To = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildEvent", x => new { x.Id, x.GuildId });
                    table.ForeignKey(
                        name: "FK_GuildEvent_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildEvent_GuildId",
                table: "GuildEvent",
                column: "GuildId");
        }
    }
}
