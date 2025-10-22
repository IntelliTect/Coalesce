using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coalesce.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeopleLocations",
                columns: table => new
                {
                    PersonLocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleLocations", x => x.PersonLocationId);
                    table.ForeignKey(
                        name: "FK_PeopleLocations_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeopleStats",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<double>(type: "float", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleStats", x => x.PersonId);
                    table.ForeignKey(
                        name: "FK_PeopleStats_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeopleLocations_PersonId",
                table: "PeopleLocations",
                column: "PersonId",
                unique: true,
                filter: "[PersonId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeopleLocations");

            migrationBuilder.DropTable(
                name: "PeopleStats");
        }
    }
}
