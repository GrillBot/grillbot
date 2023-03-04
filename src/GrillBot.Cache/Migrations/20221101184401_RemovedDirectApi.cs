using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Cache.Migrations
{
    public partial class RemovedDirectApi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectApiMessages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DirectApiMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JsonData = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectApiMessages", x => x.Id);
                });
        }
    }
}
