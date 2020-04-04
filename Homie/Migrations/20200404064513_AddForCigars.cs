using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class AddForCigars : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Cigars",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Cigars",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Cigars",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Shape",
                table: "Cigars",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Cigars");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Cigars");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Cigars");

            migrationBuilder.DropColumn(
                name: "Shape",
                table: "Cigars");
        }
    }
}
