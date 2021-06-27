using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class ReminderInContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RemindMessage_Users_FromUserId",
                table: "RemindMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_RemindMessage_Users_ToUserId",
                table: "RemindMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RemindMessage",
                table: "RemindMessage");

            migrationBuilder.RenameTable(
                name: "RemindMessage",
                newName: "Reminders");

            migrationBuilder.RenameIndex(
                name: "IX_RemindMessage_ToUserId",
                table: "Reminders",
                newName: "IX_Reminders_ToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_RemindMessage_FromUserId",
                table: "Reminders",
                newName: "IX_Reminders_FromUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_Users_FromUserId",
                table: "Reminders",
                column: "FromUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_Users_ToUserId",
                table: "Reminders",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_Users_FromUserId",
                table: "Reminders");

            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_Users_ToUserId",
                table: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders");

            migrationBuilder.RenameTable(
                name: "Reminders",
                newName: "RemindMessage");

            migrationBuilder.RenameIndex(
                name: "IX_Reminders_ToUserId",
                table: "RemindMessage",
                newName: "IX_RemindMessage_ToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reminders_FromUserId",
                table: "RemindMessage",
                newName: "IX_RemindMessage_FromUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RemindMessage",
                table: "RemindMessage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RemindMessage_Users_FromUserId",
                table: "RemindMessage",
                column: "FromUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RemindMessage_Users_ToUserId",
                table: "RemindMessage",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
