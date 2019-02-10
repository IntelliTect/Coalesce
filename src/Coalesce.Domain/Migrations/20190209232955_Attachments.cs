using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Coalesce.Domain.Migrations
{
    public partial class Attachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentName",
                table: "Case",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Case",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PlainAttachment",
                table: "Case",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentName",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Case");

            migrationBuilder.DropColumn(
                name: "PlainAttachment",
                table: "Case");
        }
    }
}
