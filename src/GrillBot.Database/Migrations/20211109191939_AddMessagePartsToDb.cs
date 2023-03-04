using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Database.Migrations
{
    public partial class AddMessagePartsToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JumpUrl",
                table: "SearchItems",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageContent",
                table: "SearchItems",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JumpUrl",
                table: "SearchItems");

            migrationBuilder.DropColumn(
                name: "MessageContent",
                table: "SearchItems");
        }
    }
}
