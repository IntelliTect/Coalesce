using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coalesce.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimitiveCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Numbers",
                table: "Case",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "States",
                table: "Case",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Strings",
                table: "Case",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Numbers",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "States",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "Strings",
                table: "Case");
        }
    }
}
