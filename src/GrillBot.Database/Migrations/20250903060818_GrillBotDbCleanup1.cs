using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrillBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class GrillBotDbCleanup1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoReplies");

            migrationBuilder.DropTable(
                name: "Nicknames");

            migrationBuilder.DropTable(
                name: "SelfunverifyKeepables");

            migrationBuilder.DropTable(
                name: "Unverifies");

            migrationBuilder.DropTable(
                name: "UnverifyLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoReplies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Flags = table.Column<long>(type: "bigint", nullable: false),
                    Reply = table.Column<string>(type: "text", nullable: false),
                    Template = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoReplies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nicknames",
                columns: table => new
                {
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    NicknameValue = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nicknames", x => new { x.GuildId, x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_Nicknames_GuildUsers_GuildId_UserId",
                        columns: x => new { x.GuildId, x.UserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelfunverifyKeepables",
                columns: table => new
                {
                    GroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfunverifyKeepables", x => new { x.GroupName, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "UnverifyLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FromUserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ToUserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnverifyLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnverifyLogs_GuildUsers_GuildId_FromUserId",
                        columns: x => new { x.GuildId, x.FromUserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnverifyLogs_GuildUsers_GuildId_ToUserId",
                        columns: x => new { x.GuildId, x.ToUserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnverifyLogs_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Unverifies",
                columns: table => new
                {
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SetOperationId = table.Column<long>(type: "bigint", nullable: false),
                    Channels = table.Column<string>(type: "jsonb", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Roles = table.Column<string>(type: "jsonb", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unverifies", x => new { x.GuildId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Unverifies_GuildUsers_GuildId_UserId",
                        columns: x => new { x.GuildId, x.UserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Unverifies_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Unverifies_UnverifyLogs_SetOperationId",
                        column: x => x.SetOperationId,
                        principalTable: "UnverifyLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Unverifies_SetOperationId",
                table: "Unverifies",
                column: "SetOperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnverifyLogs_GuildId_FromUserId",
                table: "UnverifyLogs",
                columns: new[] { "GuildId", "FromUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UnverifyLogs_GuildId_ToUserId",
                table: "UnverifyLogs",
                columns: new[] { "GuildId", "ToUserId" });
        }
    }
}
