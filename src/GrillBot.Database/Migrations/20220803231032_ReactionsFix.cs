using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class ReactionsFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PointsTransactions",
                table: "PointsTransactions");

            migrationBuilder.DropColumn(
                name: "IsReaction",
                table: "PointsTransactions");

            migrationBuilder.AddColumn<string>(
                name: "ReactionId",
                table: "PointsTransactions",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PointsTransactions",
                table: "PointsTransactions",
                columns: new[] { "GuildId", "UserId", "MessageId", "ReactionId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PointsTransactions",
                table: "PointsTransactions");

            migrationBuilder.DropColumn(
                name: "ReactionId",
                table: "PointsTransactions");

            migrationBuilder.AddColumn<bool>(
                name: "IsReaction",
                table: "PointsTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PointsTransactions",
                table: "PointsTransactions",
                columns: new[] { "GuildId", "UserId", "MessageId", "IsReaction" });
        }
    }
}
