using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace GrillBot.Database.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class SupportOnlyUserInAudit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ProcessedUserId",
                table: "AuditLogs",
                column: "ProcessedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_ProcessedUserId",
                table: "AuditLogs",
                column: "ProcessedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_ProcessedUserId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_ProcessedUserId",
                table: "AuditLogs");
        }
    }
}
