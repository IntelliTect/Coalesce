using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coalesce.Domain.Migrations;

/// <inheritdoc />
public partial class RemoveCascadeDelete : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AuditLogProperties_AuditLogs_ParentId",
            table: "AuditLogProperties");

        migrationBuilder.DropForeignKey(
            name: "FK_AuditLogs_Person_UserId",
            table: "AuditLogs");

        migrationBuilder.DropForeignKey(
            name: "FK_Case_Person_AssignedToId",
            table: "Case");

        migrationBuilder.DropForeignKey(
            name: "FK_Case_Person_ReportedById",
            table: "Case");

        migrationBuilder.DropForeignKey(
            name: "FK_CaseProduct_Case_CaseId",
            table: "CaseProduct");

        migrationBuilder.DropForeignKey(
            name: "FK_CaseProduct_Product_ProductId",
            table: "CaseProduct");

        migrationBuilder.DropForeignKey(
            name: "FK_Person_Company_CompanyId",
            table: "Person");

        migrationBuilder.AddForeignKey(
            name: "FK_AuditLogProperties_AuditLogs_ParentId",
            table: "AuditLogProperties",
            column: "ParentId",
            principalTable: "AuditLogs",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_AuditLogs_Person_UserId",
            table: "AuditLogs",
            column: "UserId",
            principalTable: "Person",
            principalColumn: "PersonId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Case_Person_AssignedToId",
            table: "Case",
            column: "AssignedToId",
            principalTable: "Person",
            principalColumn: "PersonId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Case_Person_ReportedById",
            table: "Case",
            column: "ReportedById",
            principalTable: "Person",
            principalColumn: "PersonId",
            onDelete: ReferentialAction.Restrict);

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

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AuditLogProperties_AuditLogs_ParentId",
            table: "AuditLogProperties");

        migrationBuilder.DropForeignKey(
            name: "FK_AuditLogs_Person_UserId",
            table: "AuditLogs");

        migrationBuilder.DropForeignKey(
            name: "FK_Case_Person_AssignedToId",
            table: "Case");

        migrationBuilder.DropForeignKey(
            name: "FK_Case_Person_ReportedById",
            table: "Case");

        migrationBuilder.DropForeignKey(
            name: "FK_CaseProduct_Case_CaseId",
            table: "CaseProduct");

        migrationBuilder.DropForeignKey(
            name: "FK_CaseProduct_Product_ProductId",
            table: "CaseProduct");

        migrationBuilder.DropForeignKey(
            name: "FK_Person_Company_CompanyId",
            table: "Person");

        migrationBuilder.AddForeignKey(
            name: "FK_AuditLogProperties_AuditLogs_ParentId",
            table: "AuditLogProperties",
            column: "ParentId",
            principalTable: "AuditLogs",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AuditLogs_Person_UserId",
            table: "AuditLogs",
            column: "UserId",
            principalTable: "Person",
            principalColumn: "PersonId");

        migrationBuilder.AddForeignKey(
            name: "FK_Case_Person_AssignedToId",
            table: "Case",
            column: "AssignedToId",
            principalTable: "Person",
            principalColumn: "PersonId");

        migrationBuilder.AddForeignKey(
            name: "FK_Case_Person_ReportedById",
            table: "Case",
            column: "ReportedById",
            principalTable: "Person",
            principalColumn: "PersonId");

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
}
