using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class OptionalGuildIdInLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Guilds_GuildId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "GuildId",
                table: "AuditLogs",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Guilds_GuildId",
                table: "AuditLogs",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Guilds_GuildId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "GuildId",
                table: "AuditLogs",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Guilds_GuildId",
                table: "AuditLogs",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
