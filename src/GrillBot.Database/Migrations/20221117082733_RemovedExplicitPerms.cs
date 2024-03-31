using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class RemovedExplicitPerms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExplicitPermissions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExplicitPermissions",
                columns: table => new
                {
                    Command = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsRole = table.Column<bool>(type: "boolean", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExplicitPermissions", x => new { x.Command, x.TargetId });
                });
        }
    }
}
