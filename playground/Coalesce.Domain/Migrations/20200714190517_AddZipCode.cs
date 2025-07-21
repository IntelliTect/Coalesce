using Microsoft.EntityFrameworkCore.Migrations;

namespace Coalesce.Domain.Migrations;

public partial class AddZipCode : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ZipCodes",
            columns: table => new
            {
                Zip = table.Column<string>(nullable: false),
                State = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ZipCodes", x => x.Zip);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ZipCodes");
    }
}
