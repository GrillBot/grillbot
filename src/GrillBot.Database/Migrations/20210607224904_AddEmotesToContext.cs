using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class AddEmotesToContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmoteStatisticItem_Users_UserId",
                table: "EmoteStatisticItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmoteStatisticItem",
                table: "EmoteStatisticItem");

            migrationBuilder.RenameTable(
                name: "EmoteStatisticItem",
                newName: "Emotes");

            migrationBuilder.RenameIndex(
                name: "IX_EmoteStatisticItem_UserId",
                newName: "IX_Emotes_UserId",
                table: "Emotes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emotes",
                table: "Emotes",
                column: "EmoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Emotes_Users_UserId",
                table: "Emotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emotes_Users_UserId",
                table: "Emotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Emotes",
                table: "Emotes");

            migrationBuilder.RenameTable(
                name: "Emotes",
                newName: "EmoteStatisticItem");

            migrationBuilder.RenameIndex(
                name: "IX_Emotes_UserId",
                newName: "IX_EmoteStatisticItem_UserId",
                table: "EmoteStatisticItem");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmoteStatisticItem",
                table: "EmoteStatisticItem",
                column: "EmoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmoteStatisticItem_Users_UserId",
                table: "EmoteStatisticItem",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
