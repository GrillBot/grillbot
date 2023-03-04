using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class MergePoints_Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PointsTransactions_MergedItemsCount",
                table: "PointsTransactions",
                column: "MergedItemsCount");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PointsTransactions_MergedItemsCount",
                table: "PointsTransactions");
        }
    }
}
