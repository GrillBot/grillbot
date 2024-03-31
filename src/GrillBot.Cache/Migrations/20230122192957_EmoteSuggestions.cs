using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Cache.Migrations
{
    /// <inheritdoc />
    public partial class EmoteSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmoteSuggestions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataContent = table.Column<byte[]>(type: "bytea", nullable: false),
                    Filename = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmoteSuggestions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmoteSuggestions");
        }
    }
}
