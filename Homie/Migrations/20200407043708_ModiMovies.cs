using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class ModiMovies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserUid",
                table: "MoviesEF",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserUid",
                table: "MoviesEF");
        }
    }
}
