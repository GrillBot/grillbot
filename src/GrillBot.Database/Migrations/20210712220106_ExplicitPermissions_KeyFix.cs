using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Database.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class ExplicitPermissions_KeyFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ExplicitPermissions",
                table: "ExplicitPermissions");

            migrationBuilder.DropIndex(
                name: "IX_ExplicitPermissions_TargetId_Command",
                table: "ExplicitPermissions");

            migrationBuilder.AlterColumn<string>(
                name: "Command",
                table: "ExplicitPermissions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExplicitPermissions",
                table: "ExplicitPermissions",
                columns: new[] { "Command", "TargetId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ExplicitPermissions",
                table: "ExplicitPermissions");

            migrationBuilder.AlterColumn<string>(
                name: "Command",
                table: "ExplicitPermissions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExplicitPermissions",
                table: "ExplicitPermissions",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_ExplicitPermissions_TargetId_Command",
                table: "ExplicitPermissions",
                columns: new[] { "TargetId", "Command" },
                unique: true);
        }
    }
}
