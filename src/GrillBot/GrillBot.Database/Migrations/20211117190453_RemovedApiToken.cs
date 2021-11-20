using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class RemovedApiToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_ApiToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApiToken",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApiToken",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApiToken",
                table: "Users",
                column: "ApiToken",
                unique: true);
        }
    }
}
