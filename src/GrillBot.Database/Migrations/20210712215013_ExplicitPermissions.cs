using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class ExplicitPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExplicitPermissions",
                columns: table => new
                {
                    TargetId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsRole = table.Column<bool>(type: "boolean", nullable: false),
                    Command = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Allowed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExplicitPermissions", x => x.TargetId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExplicitPermissions_TargetId_Command",
                table: "ExplicitPermissions",
                columns: new[] { "TargetId", "Command" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExplicitPermissions");
        }
    }
}
