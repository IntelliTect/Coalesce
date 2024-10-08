using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coalesce.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Product SET ProductUniqueId = NEWID()");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductUniqueId",
                table: "Product",
                column: "ProductUniqueId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Product_ProductUniqueId",
                table: "Product");
        }
    }
}
