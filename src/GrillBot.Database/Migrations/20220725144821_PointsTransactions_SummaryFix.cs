using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class PointsTransactions_SummaryFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PointsTransactionSummaries",
                table: "PointsTransactionSummaries");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PointsTransactionSummaries",
                table: "PointsTransactionSummaries",
                columns: new[] { "GuildId", "UserId", "Day" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PointsTransactionSummaries",
                table: "PointsTransactionSummaries");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PointsTransactionSummaries",
                table: "PointsTransactionSummaries",
                columns: new[] { "GuildId", "UserId" });
        }
    }
}
