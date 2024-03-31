using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class UserNicknamesInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nicknames");
        }
    }
}
