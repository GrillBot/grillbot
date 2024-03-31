using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class RemovedUnusedSearchingProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JumpUrl",
                table: "SearchItems");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "SearchItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JumpUrl",
                table: "SearchItems",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageId",
                table: "SearchItems",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }
    }
}
