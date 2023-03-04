using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class RemovedUselessFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebAdminBannedTo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WebAdminLoginCount",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "WebAdminBannedTo",
                table: "Users",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WebAdminLoginCount",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
