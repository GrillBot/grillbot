using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Cache.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "MessageIndex",
                columns: table => new
                {
                    MessageId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ChannelId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AuthorId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageIndex", x => x.MessageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageCache_AuthorId",
                table: "MessageIndex",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageCache_GuildId",
                table: "MessageIndex",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageCache_ChannelId",
                table: "MessageIndex",
                column: "ChannelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectApiMessages");

            migrationBuilder.DropTable(
                name: "MessageIndex");
        }
    }
}
