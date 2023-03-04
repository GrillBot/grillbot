using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class PointsCooldownToDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPointsMessageIncrement",
                table: "GuildUsers",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPointsReactionIncrement",
                table: "GuildUsers",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPointsMessageIncrement",
                table: "GuildUsers");

            migrationBuilder.DropColumn(
                name: "LastPointsReactionIncrement",
                table: "GuildUsers");
        }
    }
}
