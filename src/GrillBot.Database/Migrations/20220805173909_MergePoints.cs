using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class MergePoints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MergeRangeFrom",
                table: "PointsTransactionSummaries",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MergeRangeTo",
                table: "PointsTransactionSummaries",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MergeRangeFrom",
                table: "PointsTransactions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MergeRangeTo",
                table: "PointsTransactions",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MergeRangeFrom",
                table: "PointsTransactionSummaries");

            migrationBuilder.DropColumn(
                name: "MergeRangeTo",
                table: "PointsTransactionSummaries");

            migrationBuilder.DropColumn(
                name: "MergeRangeFrom",
                table: "PointsTransactions");

            migrationBuilder.DropColumn(
                name: "MergeRangeTo",
                table: "PointsTransactions");
        }
    }
}
