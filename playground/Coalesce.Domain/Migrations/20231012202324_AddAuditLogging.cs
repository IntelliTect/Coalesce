using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coalesce.Domain.Migrations
{
    public partial class AddAuditLogging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "ZipCodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Case",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ObjectChanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    KeyValue = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    State = table.Column<byte>(type: "tinyint", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectChanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjectChangeProperties",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<long>(type: "bigint", nullable: false),
                    PropertyName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectChangeProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObjectChangeProperties_ObjectChanges_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ObjectChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObjectChangeProperties_ParentId",
                table: "ObjectChangeProperties",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectChanges_State",
                table: "ObjectChanges",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectChanges_Type",
                table: "ObjectChanges",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectChanges_Type_KeyValue",
                table: "ObjectChanges",
                columns: new[] { "Type", "KeyValue" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObjectChangeProperties");

            migrationBuilder.DropTable(
                name: "ObjectChanges");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "ZipCodes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Case",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
