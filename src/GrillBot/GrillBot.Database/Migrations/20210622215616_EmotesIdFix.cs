using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Database.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class EmotesIdFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emotes_Users_UserId",
                table: "Emotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Emotes",
                table: "Emotes");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Emotes",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emotes",
                table: "Emotes",
                columns: new[] { "EmoteId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Emotes_Users_UserId",
                table: "Emotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emotes_Users_UserId",
                table: "Emotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Emotes",
                table: "Emotes");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Emotes",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emotes",
                table: "Emotes",
                column: "EmoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Emotes_Users_UserId",
                table: "Emotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
