using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Cache.Migrations
{
    public partial class DataCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataCache",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataCache", x => x.Key);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataCache");
        }
    }
}
