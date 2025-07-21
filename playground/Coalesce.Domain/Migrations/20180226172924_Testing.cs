using Microsoft.EntityFrameworkCore.Migrations;

namespace Coalesce.Domain.Migrations;

public partial class Testing : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Details_CompanyHqAddress_Address",
            table: "Product",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Details_CompanyHqAddress_City",
            table: "Product",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Details_CompanyHqAddress_PostalCode",
            table: "Product",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Details_CompanyHqAddress_State",
            table: "Product",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Details_ManufacturingAddress_Address",
            table: "Product",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Details_ManufacturingAddress_City",
            table: "Product",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Details_ManufacturingAddress_PostalCode",
            table: "Product",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Details_ManufacturingAddress_State",
            table: "Product",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Details_CompanyHqAddress_Address",
            table: "Product");

        migrationBuilder.DropColumn(
            name: "Details_CompanyHqAddress_City",
            table: "Product");

        migrationBuilder.DropColumn(
            name: "Details_CompanyHqAddress_PostalCode",
            table: "Product");

        migrationBuilder.DropColumn(
            name: "Details_CompanyHqAddress_State",
            table: "Product");

        migrationBuilder.DropColumn(
            name: "Details_ManufacturingAddress_Address",
            table: "Product");

        migrationBuilder.DropColumn(
            name: "Details_ManufacturingAddress_City",
            table: "Product");

        migrationBuilder.DropColumn(
            name: "Details_ManufacturingAddress_PostalCode",
            table: "Product");

        migrationBuilder.DropColumn(
            name: "Details_ManufacturingAddress_State",
            table: "Product");
    }
}
