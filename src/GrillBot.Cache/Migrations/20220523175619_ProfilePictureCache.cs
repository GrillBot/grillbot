using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Cache.Migrations
{
    public partial class ProfilePictureCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfilePictures",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Size = table.Column<short>(type: "smallint", nullable: false),
                    AvatarId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsAnimated = table.Column<bool>(type: "boolean", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfilePictures", x => new { x.UserId, x.Size, x.AvatarId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfilePictures");
        }
    }
}
