using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class GuildIdInMessageCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE public.\"MessageCacheIndexes\"");

            migrationBuilder.AddColumn<string>(
                name: "GuildId",
                table: "MessageCacheIndexes",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MessageCache_GuildId",
                table: "MessageCacheIndexes",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessageCache_GuildId",
                table: "MessageCacheIndexes");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "MessageCacheIndexes");
        }
    }
}
