using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class SeriesAddPropsMoviesModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Avatar",
                table: "MoviesEF",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImgBT",
                table: "MoviesEF",
                type: "varchar(255)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "MoviesEF");

            migrationBuilder.DropColumn(
                name: "ImgBT",
                table: "MoviesEF");
        }
    }
}
