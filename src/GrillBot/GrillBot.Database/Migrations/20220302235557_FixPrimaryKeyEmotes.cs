using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace GrillBot.Database.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class FixPrimaryKeyEmotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Emotes",
                table: "Emotes");

            migrationBuilder.DropColumn("GuildId", "Emotes");

            migrationBuilder.AddColumn<string>(
                name: "GuildId",
                table: "Emotes",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emotes",
                table: "Emotes",
                columns: new[] { "EmoteId", "UserId", "GuildId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Emotes",
                table: "Emotes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emotes",
                table: "Emotes",
                columns: new[] { "EmoteId", "UserId" });
        }
    }
}
