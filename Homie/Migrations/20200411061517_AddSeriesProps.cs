using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class AddSeriesProps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "MoviesEF",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Href",
                table: "MoviesEF",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "MoviesEF");

            migrationBuilder.DropColumn(
                name: "Href",
                table: "MoviesEF");
        }
    }
}
