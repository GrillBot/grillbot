using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class EmoteSuggestionsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmoteSuggestions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SuggestionMessageId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VoteMessageId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    VoteEndsAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ImageData = table.Column<byte[]>(type: "bytea", nullable: false),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FromUserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EmoteName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    VoteUp = table.Column<int>(type: "integer", nullable: false),
                    VoteDown = table.Column<int>(type: "integer", nullable: false),
                    ApprovedForVote = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmoteSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmoteSuggestions_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmoteSuggestions_GuildUsers_GuildId_FromUserId",
                        columns: x => new { x.GuildId, x.FromUserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmoteSuggestions_GuildId_FromUserId",
                table: "EmoteSuggestions",
                columns: new[] { "GuildId", "FromUserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmoteSuggestions");
        }
    }
}
