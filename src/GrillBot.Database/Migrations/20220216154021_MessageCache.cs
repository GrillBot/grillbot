using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class MessageCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageCacheIndexes",
                columns: table => new
                {
                    MessageId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ChannelId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AuthorId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageCacheIndexes", x => x.MessageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageCache_AuthorId",
                table: "MessageCacheIndexes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageCache_ChannelId",
                table: "MessageCacheIndexes",
                column: "ChannelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageCacheIndexes");
        }
    }
}
