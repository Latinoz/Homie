using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class DelShapeId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShapeId",
                table: "CigarsEF");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShapeId",
                table: "CigarsEF",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
