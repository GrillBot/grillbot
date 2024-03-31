using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class SuggestionGuildId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuildId",
                table: "Suggestions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Suggestions");
        }
    }
}
