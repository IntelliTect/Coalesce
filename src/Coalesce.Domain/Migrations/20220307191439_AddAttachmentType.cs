using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Coalesce.Domain.Migrations
{
    public partial class AddAttachmentType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "AttachmentHash",
                table: "Case",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentType",
                table: "Case",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentType",
                table: "Case");

            migrationBuilder.AlterColumn<byte[]>(
                name: "AttachmentHash",
                table: "Case",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldMaxLength: 32,
                oldNullable: true);
        }
    }
}
