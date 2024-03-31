using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GrillBot.Database.Migrations
{
    public partial class AutoReply : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoReplies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Template = table.Column<string>(type: "text", nullable: false),
                    Reply = table.Column<string>(type: "text", nullable: false),
                    Flags = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoReplies", x => x.Id);
                });

            migrationBuilder.InsertData("AutoReplies", columns: new[] { "Template", "Reply", "Flags" }, values: new object[] { "uh oh", "uh oh", 1 });
            migrationBuilder.InsertData("AutoReplies", columns: new[] { "Template", "Reply", "Flags" }, values: new object[] { "^PR$", "https://gitlab.com/grillbot/grillbot", 3 });
            migrationBuilder.InsertData("AutoReplies", columns: new[] { "Template", "Reply", "Flags" }, values: new object[] { "^ISSUE$", "https://gitlab.com/grillbot/grillbot/-/issues", 3 });
            migrationBuilder.InsertData("AutoReplies", columns: new[] { "Template", "Reply", "Flags" }, values: new object[] { "^Je čerstvá!$", "Není čerstvá! <:reee:484470874561576960>", 3 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoReplies");
        }
    }
}
