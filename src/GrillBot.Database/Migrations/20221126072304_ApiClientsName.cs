using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrillBot.Database.Migrations
{
    public partial class ApiClientsName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ApiClients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
            
            migrationBuilder.Sql("UPDATE public.\"ApiClients\" SET \"Name\"=\"Id\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ApiClients");
        }
    }
}
