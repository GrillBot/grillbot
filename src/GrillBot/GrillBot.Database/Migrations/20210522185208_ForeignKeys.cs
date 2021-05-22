using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class ForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_SearchItems_Guilds_GuildId",
                table: "SearchItems",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Unverifies_Guilds_GuildId",
                table: "Unverifies",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UnverifyLogs_Guilds_GuildId",
                table: "UnverifyLogs",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchItems_Guilds_GuildId",
                table: "SearchItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Unverifies_Guilds_GuildId",
                table: "Unverifies");

            migrationBuilder.DropForeignKey(
                name: "FK_UnverifyLogs_Guilds_GuildId",
                table: "UnverifyLogs");
        }
    }
}
