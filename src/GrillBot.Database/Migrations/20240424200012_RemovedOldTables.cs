using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrillBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemovedOldTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Emotes");

            migrationBuilder.DropTable(
                name: "EmoteSuggestions");

            migrationBuilder.DropColumn(
                name: "EmoteSuggestionChannelId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "EmoteSuggestionsFrom",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "EmoteSuggestionsTo",
                table: "Guilds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmoteSuggestionChannelId",
                table: "Guilds",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmoteSuggestionsFrom",
                table: "Guilds",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmoteSuggestionsTo",
                table: "Guilds",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Emotes",
                columns: table => new
                {
                    EmoteId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FirstOccurence = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsEmoteSupported = table.Column<bool>(type: "boolean", nullable: false),
                    LastOccurence = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UseCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emotes", x => new { x.EmoteId, x.UserId, x.GuildId });
                    table.ForeignKey(
                        name: "FK_Emotes_GuildUsers_GuildId_UserId",
                        columns: x => new { x.GuildId, x.UserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Emotes_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmoteSuggestions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FromUserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ApprovedForVote = table.Column<bool>(type: "boolean", nullable: true),
                    CommunityApproved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    DownVotes = table.Column<int>(type: "integer", nullable: false),
                    EmoteName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ImageData = table.Column<byte[]>(type: "bytea", nullable: false),
                    SuggestionMessageId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UpVotes = table.Column<int>(type: "integer", nullable: false),
                    VoteEndsAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    VoteFinished = table.Column<bool>(type: "boolean", nullable: false),
                    VoteMessageId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmoteSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmoteSuggestions_GuildUsers_GuildId_FromUserId",
                        columns: x => new { x.GuildId, x.FromUserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmoteSuggestions_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Emotes_GuildId_UserId",
                table: "Emotes",
                columns: new[] { "GuildId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmoteSuggestions_GuildId_FromUserId",
                table: "EmoteSuggestions",
                columns: new[] { "GuildId", "FromUserId" });
        }
    }
}
