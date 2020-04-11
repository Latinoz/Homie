using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class ModSeriesPlay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StopPlayHour",
                table: "MoviesEF");

            migrationBuilder.DropColumn(
                name: "StopPlayMinute",
                table: "MoviesEF");

            migrationBuilder.DropColumn(
                name: "StopPlaySecond",
                table: "MoviesEF");

            migrationBuilder.AddColumn<string>(
                name: "HoldPlay",
                table: "MoviesEF",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoldPlay",
                table: "MoviesEF");

            migrationBuilder.AddColumn<int>(
                name: "StopPlayHour",
                table: "MoviesEF",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StopPlayMinute",
                table: "MoviesEF",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StopPlaySecond",
                table: "MoviesEF",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
