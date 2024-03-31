using Microsoft.EntityFrameworkCore.Migrations;

namespace GrillBot.Database.Migrations
{
    public partial class ChannelFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Flags",
                table: "Channels",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flags",
                table: "Channels");
        }
    }
}
