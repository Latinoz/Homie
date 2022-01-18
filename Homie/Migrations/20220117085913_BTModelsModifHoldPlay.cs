using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTModelsModifHoldPlay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "HoldPlay",
                table: "MoviesEF",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HoldPlay",
                table: "MoviesEF",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
