using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class MergePoints_Counts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MergedItemsCount",
                table: "PointsTransactionSummaries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MergedItemsCount",
                table: "PointsTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MergedItemsCount",
                table: "PointsTransactionSummaries");

            migrationBuilder.DropColumn(
                name: "MergedItemsCount",
                table: "PointsTransactions");
        }
    }
}
