using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coalesce.Domain.Migrations;

/// <inheritdoc />
public partial class AddAbstractModelPerson : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AbstractClassPeople",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PersonId = table.Column<int>(type: "int", nullable: false),
                AbstractClassId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AbstractClassPeople", x => x.Id);
                table.ForeignKey(
                    name: "FK_AbstractClassPeople_AbstractClasses_AbstractClassId",
                    column: x => x.AbstractClassId,
                    principalTable: "AbstractClasses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AbstractClassPeople_Person_PersonId",
                    column: x => x.PersonId,
                    principalTable: "Person",
                    principalColumn: "PersonId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AbstractClassPeople_AbstractClassId",
            table: "AbstractClassPeople",
            column: "AbstractClassId");

        migrationBuilder.CreateIndex(
            name: "IX_AbstractClassPeople_PersonId",
            table: "AbstractClassPeople",
            column: "PersonId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AbstractClassPeople");
    }
}
