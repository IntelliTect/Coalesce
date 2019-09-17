using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Coalesce.Domain.Migrations
{
    public partial class MissingMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Title",
                table: "Person",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "InternalUseFileName",
                table: "Case",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "InternalUseFileSize",
                table: "Case",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<byte[]>(
                name: "RestrictedDownloadAttachment",
                table: "Case",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RestrictedMetaAttachment",
                table: "Case",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RestrictedUploadAttachment",
                table: "Case",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternalUseFileName",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "InternalUseFileSize",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "RestrictedDownloadAttachment",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "RestrictedMetaAttachment",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "RestrictedUploadAttachment",
                table: "Case");

            migrationBuilder.AlterColumn<int>(
                name: "Title",
                table: "Person",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
