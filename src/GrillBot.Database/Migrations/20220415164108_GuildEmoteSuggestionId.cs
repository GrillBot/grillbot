using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class GuildEmoteSuggestionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmoteSuggestionChannelId",
                table: "Guilds",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmoteSuggestionChannelId",
                table: "Guilds");
        }
    }
}
