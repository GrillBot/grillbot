using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class GuildEvents_Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildEvent",
                table: "GuildEvent");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildEvent",
                table: "GuildEvent",
                columns: new[] { "Id", "GuildId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildEvent",
                table: "GuildEvent");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildEvent",
                table: "GuildEvent",
                column: "Id");
        }
    }
}
