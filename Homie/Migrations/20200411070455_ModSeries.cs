using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class ModSeries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Href",
                table: "MoviesEF");

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "MoviesEF",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Link",
                table: "MoviesEF");

            migrationBuilder.AddColumn<string>(
                name: "Href",
                table: "MoviesEF",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
