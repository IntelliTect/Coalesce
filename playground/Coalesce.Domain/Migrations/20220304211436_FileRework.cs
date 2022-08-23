using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Coalesce.Domain.Migrations
{
    public partial class FileRework : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attachment",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "ImageHash",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "ImageSize",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "InternalUseFileName",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "PlainAttachment",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "RestrictedDownloadAttachment",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "RestrictedMetaAttachment",
                table: "Case");

            migrationBuilder.RenameColumn(
                name: "RestrictedUploadAttachment",
                table: "Case",
                newName: "AttachmentHash");

            migrationBuilder.RenameColumn(
                name: "InternalUseFileSize",
                table: "Case",
                newName: "AttachmentSize");

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "Case",
                nullable: false,
                defaultValue: new byte[] {  });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Case");

            migrationBuilder.RenameColumn(
                name: "AttachmentSize",
                table: "Case",
                newName: "InternalUseFileSize");

            migrationBuilder.RenameColumn(
                name: "AttachmentHash",
                table: "Case",
                newName: "RestrictedUploadAttachment");

            migrationBuilder.AddColumn<byte[]>(
                name: "Attachment",
                table: "Case",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Case",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageHash",
                table: "Case",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ImageSize",
                table: "Case",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "InternalUseFileName",
                table: "Case",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PlainAttachment",
                table: "Case",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RestrictedDownloadAttachment",
                table: "Case",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RestrictedMetaAttachment",
                table: "Case",
                nullable: true);
        }
    }
}
