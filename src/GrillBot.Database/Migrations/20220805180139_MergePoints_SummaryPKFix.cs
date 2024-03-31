using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class MergePoints_SummaryPKFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PointsTransactionSummaries",
                table: "PointsTransactionSummaries");

            migrationBuilder.AddColumn<bool>(
                name: "IsMerged",
                table: "PointsTransactionSummaries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PointsTransactionSummaries",
                table: "PointsTransactionSummaries",
                columns: new[] { "GuildId", "UserId", "Day", "IsMerged" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PointsTransactionSummaries",
                table: "PointsTransactionSummaries");

            migrationBuilder.DropColumn(
                name: "IsMerged",
                table: "PointsTransactionSummaries");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PointsTransactionSummaries",
                table: "PointsTransactionSummaries",
                columns: new[] { "GuildId", "UserId", "Day" });
        }
    }
}
