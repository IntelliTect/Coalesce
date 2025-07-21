using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Coalesce.Domain.Migrations;

public partial class AddTimeSpan : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PersonStatsId",
            table: "Person");

        migrationBuilder.AddColumn<TimeSpan>(
            name: "Duration",
            table: "Case",
            nullable: false,
            defaultValue: new TimeSpan(0, 0, 0, 0, 0));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Duration",
            table: "Case");

        migrationBuilder.AddColumn<int>(
            name: "PersonStatsId",
            table: "Person",
            nullable: true);
    }
}
