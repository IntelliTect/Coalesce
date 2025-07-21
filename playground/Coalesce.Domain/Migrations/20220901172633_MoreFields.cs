using Microsoft.EntityFrameworkCore.Migrations;

namespace Coalesce.Domain.Migrations;

public partial class MoreFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "LogoUrl",
            table: "Company",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Phone",
            table: "Company",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "WebsiteUrl",
            table: "Company",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LogoUrl",
            table: "Company");

        migrationBuilder.DropColumn(
            name: "Phone",
            table: "Company");

        migrationBuilder.DropColumn(
            name: "WebsiteUrl",
            table: "Company");
    }
}
