using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Cache.Migrations
{
    public partial class InviteMetadataCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InviteMetadata",
                columns: table => new
                {
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Uses = table.Column<int>(type: "integer", nullable: false),
                    IsVanity = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteMetadata", x => new { x.GuildId, x.Code });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InviteMetadata");
        }
    }
}
