using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Database.Migrations
{
    public partial class ReminderMessageId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RemindMessageId",
                table: "RemindMessage",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemindMessageId",
                table: "RemindMessage");
        }
    }
}
