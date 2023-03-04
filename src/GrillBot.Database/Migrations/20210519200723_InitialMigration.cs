using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GrillBot.Database.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Flags = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ApiToken = table.Column<Guid>(type: "uuid", nullable: false),
                    Flags = table.Column<int>(type: "integer", nullable: false),
                    Birthday = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WebAdminLoginCount = table.Column<int>(type: "integer", nullable: false),
                    WebAdminBannedTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmoteStatisticItem",
                columns: table => new
                {
                    EmoteId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    UseCount = table.Column<long>(type: "bigint", nullable: false),
                    FirstOccurence = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastOccurence = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmoteStatisticItem", x => x.EmoteId);
                    table.ForeignKey(
                        name: "FK_EmoteStatisticItem_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuildChannel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Count = table.Column<long>(type: "bigint", nullable: false),
                    FirstMessageAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildChannel", x => new { x.GuildId, x.Id });
                    table.ForeignKey(
                        name: "FK_GuildChannel_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuildChannel_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RemindMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromUserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ToUserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    At = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Postpone = table.Column<int>(type: "integer", nullable: false),
                    OriginalMessageId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemindMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemindMessage_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RemindMessage_Users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SearchItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ChannelId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MessageId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchItems_GuildChannel_GuildId_ChannelId",
                        columns: x => new { x.GuildId, x.ChannelId },
                        principalTable: "GuildChannel",
                        principalColumns: new[] { "GuildId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SearchItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ProcessedUserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DiscordAuditLogItemId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ChannelId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_GuildChannel_GuildId_ChannelId",
                        columns: x => new { x.GuildId, x.ChannelId },
                        principalTable: "GuildChannel",
                        principalColumns: new[] { "GuildId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuildUsers",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UsedInviteCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Points = table.Column<long>(type: "bigint", nullable: false),
                    GivenReactions = table.Column<long>(type: "bigint", nullable: false),
                    ObtainedReactions = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildUsers", x => new { x.GuildId, x.UserId });
                    table.ForeignKey(
                        name: "FK_GuildUsers_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuildUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invites",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatorId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invites", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Invites_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invites_GuildUsers_GuildId_CreatorId",
                        columns: x => new { x.GuildId, x.CreatorId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnverifyLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FromUserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ToUserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Unverifies",
                columns: table => new
                {
                    GuildId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Roles = table.Column<List<string>>(type: "jsonb", nullable: true),
                    Channels = table.Column<List<string>>(type: "jsonb", nullable: true),
                    SetOperationId = table.Column<long>(type: "bigint", nullable: false)
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
                        name: "FK_Unverifies_UnverifyLogs_SetOperationId",
                        column: x => x.SetOperationId,
                        principalTable: "UnverifyLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_GuildId_ChannelId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ChannelId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_GuildId_ProcessedUserId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ProcessedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmoteStatisticItem_UserId",
                table: "EmoteStatisticItem",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildChannel_UserId",
                table: "GuildChannel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildUsers_UsedInviteCode",
                table: "GuildUsers",
                column: "UsedInviteCode");

            migrationBuilder.CreateIndex(
                name: "IX_GuildUsers_UserId",
                table: "GuildUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invites_GuildId_CreatorId",
                table: "Invites",
                columns: new[] { "GuildId", "CreatorId" });

            migrationBuilder.CreateIndex(
                name: "IX_RemindMessage_FromUserId",
                table: "RemindMessage",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RemindMessage_ToUserId",
                table: "RemindMessage",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchItems_GuildId_ChannelId",
                table: "SearchItems",
                columns: new[] { "GuildId", "ChannelId" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchItems_UserId",
                table: "SearchItems",
                column: "UserId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApiToken",
                table: "Users",
                column: "ApiToken",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_GuildUsers_GuildId_ProcessedUserId",
                table: "AuditLogs",
                columns: new[] { "GuildId", "ProcessedUserId" },
                principalTable: "GuildUsers",
                principalColumns: new[] { "GuildId", "UserId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildUsers_Invites_UsedInviteCode",
                table: "GuildUsers",
                column: "UsedInviteCode",
                principalTable: "Invites",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_GuildUsers_GuildId_CreatorId",
                table: "Invites");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "EmoteStatisticItem");

            migrationBuilder.DropTable(
                name: "RemindMessage");

            migrationBuilder.DropTable(
                name: "SearchItems");

            migrationBuilder.DropTable(
                name: "Unverifies");

            migrationBuilder.DropTable(
                name: "GuildChannel");

            migrationBuilder.DropTable(
                name: "UnverifyLogs");

            migrationBuilder.DropTable(
                name: "GuildUsers");

            migrationBuilder.DropTable(
                name: "Invites");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
