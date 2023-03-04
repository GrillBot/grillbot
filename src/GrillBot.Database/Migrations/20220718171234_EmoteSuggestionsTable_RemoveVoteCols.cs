using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class EmoteSuggestionsTable_RemoveVoteCols : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoteDown",
                table: "EmoteSuggestions");

            migrationBuilder.DropColumn(
                name: "VoteUp",
                table: "EmoteSuggestions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VoteDown",
                table: "EmoteSuggestions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VoteUp",
                table: "EmoteSuggestions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
