using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class ExplicitPermissionState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "ExplicitPermissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "ExplicitPermissions");
        }
    }
}
