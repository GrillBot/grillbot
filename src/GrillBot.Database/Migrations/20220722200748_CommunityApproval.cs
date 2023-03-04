using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class CommunityApproval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CommunityApproved",
                table: "EmoteSuggestions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DownVotes",
                table: "EmoteSuggestions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpVotes",
                table: "EmoteSuggestions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommunityApproved",
                table: "EmoteSuggestions");

            migrationBuilder.DropColumn(
                name: "DownVotes",
                table: "EmoteSuggestions");

            migrationBuilder.DropColumn(
                name: "UpVotes",
                table: "EmoteSuggestions");
        }
    }
}
