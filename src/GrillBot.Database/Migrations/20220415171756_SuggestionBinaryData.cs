using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class SuggestionBinaryData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "BinaryData",
                table: "Suggestions",
                type: "bytea",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BinaryData",
                table: "Suggestions");
        }
    }
}
