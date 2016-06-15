using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Coalesce.Domain.Migrations
{
    public partial class PersonStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_CaseProduct_Case_CaseId", table: "CaseProduct");
            migrationBuilder.DropForeignKey(name: "FK_CaseProduct_Product_ProductId", table: "CaseProduct");
            migrationBuilder.DropForeignKey(name: "FK_Person_Company_CompanyId", table: "Person");
            migrationBuilder.AddColumn<int>(
                name: "PersonStatsId",
                table: "Person",
                nullable: false,
                defaultValue: 0);
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
            migrationBuilder.DropColumn(name: "PersonStatsId", table: "Person");
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
}
