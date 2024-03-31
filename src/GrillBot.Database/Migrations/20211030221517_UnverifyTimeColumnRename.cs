using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class UnverifyTimeColumnRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnverifyMinimalTime",
                table: "Users",
                newName: "SelfUnverifyMinimalTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SelfUnverifyMinimalTime",
                table: "Users",
                newName: "UnverifyMinimalTime");
        }
    }
}
