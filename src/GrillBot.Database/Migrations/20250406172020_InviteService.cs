using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class InviteService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildUsers_Invites_UsedInviteCode",
                table: "GuildUsers");

            migrationBuilder.DropTable(
                name: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_GuildUsers_UsedInviteCode",
                table: "GuildUsers");

            migrationBuilder.DropColumn(
                name: "UsedInviteCode",
                table: "GuildUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsedInviteCode",
                table: "GuildUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Invites",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatorId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invites", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Invites_GuildUsers_GuildId_CreatorId",
                        columns: x => new { x.GuildId, x.CreatorId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" });
                    table.ForeignKey(
                        name: "FK_Invites_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildUsers_UsedInviteCode",
                table: "GuildUsers",
                column: "UsedInviteCode");

            migrationBuilder.CreateIndex(
                name: "IX_Invites_GuildId_CreatorId",
                table: "Invites",
                columns: new[] { "GuildId", "CreatorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_GuildUsers_Invites_UsedInviteCode",
                table: "GuildUsers",
                column: "UsedInviteCode",
                principalTable: "Invites",
                principalColumn: "Code");
        }
    }
}
