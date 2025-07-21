using Microsoft.EntityFrameworkCore.Migrations;

namespace Coalesce.Domain.Migrations;

public partial class AddCompanyIsDeletedFlag : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Company",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Company");
    }
}
