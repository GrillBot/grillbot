using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemovedOldColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointsTransactions");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastPointsMessageIncrement",
                table: "GuildUsers");

            migrationBuilder.DropColumn(
                name: "LastPointsReactionIncrement",
                table: "GuildUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPointsMessageIncrement",
                table: "GuildUsers",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPointsReactionIncrement",
                table: "GuildUsers",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PointsTransactions",
                columns: table => new
                {
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MessageId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReactionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AssingnedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MergeRangeFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MergeRangeTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MergedItemsCount = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsTransactions", x => new { x.GuildId, x.UserId, x.MessageId, x.ReactionId });
                    table.ForeignKey(
                        name: "FK_PointsTransactions_GuildUsers_GuildId_UserId",
                        columns: x => new { x.GuildId, x.UserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PointsTransactions_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransactions_AssignedAt",
                table: "PointsTransactions",
                column: "AssingnedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransactions_MergedItemsCount",
                table: "PointsTransactions",
                column: "MergedItemsCount");
        }
    }
}
