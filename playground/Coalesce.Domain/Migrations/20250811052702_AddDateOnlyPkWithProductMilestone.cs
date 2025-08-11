using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coalesce.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddDateOnlyPkWithProductMilestone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "MilestoneId",
                table: "Product",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DateOnlyPks",
                columns: table => new
                {
                    DateOnlyPkId = table.Column<DateOnly>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateOnlyPks", x => x.DateOnlyPkId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Product_MilestoneId",
                table: "Product",
                column: "MilestoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_DateOnlyPks_MilestoneId",
                table: "Product",
                column: "MilestoneId",
                principalTable: "DateOnlyPks",
                principalColumn: "DateOnlyPkId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_DateOnlyPks_MilestoneId",
                table: "Product");

            migrationBuilder.DropTable(
                name: "DateOnlyPks");

            migrationBuilder.DropIndex(
                name: "IX_Product_MilestoneId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "MilestoneId",
                table: "Product");
        }
    }
}
