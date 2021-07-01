using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTModelsModNameImg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Picture");

            migrationBuilder.AddColumn<string>(
                name: "NameImg",
                table: "Picture",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameImg",
                table: "Picture");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Picture",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
