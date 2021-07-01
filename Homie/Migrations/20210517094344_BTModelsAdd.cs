using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTModelsAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImgMech",
                table: "BTMechsEF");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImgBT",
                table: "BTMechsEF",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImgBT",
                table: "BTMechsEF");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImgMech",
                table: "BTMechsEF",
                type: "longblob",
                nullable: true);
        }
    }
}
