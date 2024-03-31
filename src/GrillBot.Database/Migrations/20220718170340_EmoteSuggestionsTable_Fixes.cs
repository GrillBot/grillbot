using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class EmoteSuggestionsTable_Fixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "EmoteSuggestions",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ApprovedForVote",
                table: "EmoteSuggestions",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "EmoteSuggestions",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ApprovedForVote",
                table: "EmoteSuggestions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);
        }
    }
}
