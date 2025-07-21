using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Coalesce.Domain.Migrations;

public partial class Dates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_CaseProduct_Case_CaseId", table: "CaseProduct");
        migrationBuilder.DropForeignKey(name: "FK_CaseProduct_Product_ProductId", table: "CaseProduct");
        migrationBuilder.DropForeignKey(name: "FK_Person_Company_CompanyId", table: "Person");
        migrationBuilder.AddColumn<DateTime>(
            name: "BirthDate",
            table: "Person",
            nullable: true);
        migrationBuilder.AddColumn<DateTime>(
            name: "LastBath",
            table: "Person",
            nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "NextUpgrade",
            table: "Person",
            nullable: true);
        migrationBuilder.AddForeignKey(
            name: "FK_CaseProduct_Case_CaseId",
            table: "CaseProduct",
            column: "CaseId",
            principalTable: "Case",
            principalColumn: "CaseKey",
            onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey(
            name: "FK_CaseProduct_Product_ProductId",
            table: "CaseProduct",
            column: "ProductId",
            principalTable: "Product",
            principalColumn: "ProductId",
            onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey(
            name: "FK_Person_Company_CompanyId",
            table: "Person",
            column: "CompanyId",
            principalTable: "Company",
            principalColumn: "CompanyId",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_CaseProduct_Case_CaseId", table: "CaseProduct");
        migrationBuilder.DropForeignKey(name: "FK_CaseProduct_Product_ProductId", table: "CaseProduct");
        migrationBuilder.DropForeignKey(name: "FK_Person_Company_CompanyId", table: "Person");
        migrationBuilder.DropColumn(name: "BirthDate", table: "Person");
        migrationBuilder.DropColumn(name: "LastBath", table: "Person");
        migrationBuilder.DropColumn(name: "NextUpgrade", table: "Person");
        migrationBuilder.AddForeignKey(
            name: "FK_CaseProduct_Case_CaseId",
            table: "CaseProduct",
            column: "CaseId",
            principalTable: "Case",
            principalColumn: "CaseKey",
            onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(
            name: "FK_CaseProduct_Product_ProductId",
            table: "CaseProduct",
            column: "ProductId",
            principalTable: "Product",
            principalColumn: "ProductId",
            onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(
            name: "FK_Person_Company_CompanyId",
            table: "Person",
            column: "CompanyId",
            principalTable: "Company",
            principalColumn: "CompanyId",
            onDelete: ReferentialAction.Restrict);
    }
}
