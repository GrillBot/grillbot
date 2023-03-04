using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class UserLanguageInEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointsTransactionSummaries");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "PointsTransactionSummaries",
                columns: table => new
                {
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Day = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsMerged = table.Column<bool>(type: "boolean", nullable: false),
                    MergeRangeFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MergeRangeTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MergedItemsCount = table.Column<int>(type: "integer", nullable: false),
                    MessagePoints = table.Column<long>(type: "bigint", nullable: false),
                    ReactionPoints = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsTransactionSummaries", x => new { x.GuildId, x.UserId, x.Day, x.IsMerged });
                    table.ForeignKey(
                        name: "FK_PointsTransactionSummaries_GuildUsers_GuildId_UserId",
                        columns: x => new { x.GuildId, x.UserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PointsTransactionSummaries_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
