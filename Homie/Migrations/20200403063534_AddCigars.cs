using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class AddCigars : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cigars",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Length = table.Column<string>(nullable: true),
                    Ring = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    Filler = table.Column<string>(nullable: true),
                    Wrapper = table.Column<string>(nullable: true),
                    Color = table.Column<string>(nullable: true),
                    Strength = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cigars", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cigars");
        }
    }
}
