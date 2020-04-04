using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class Modifi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cigars");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.CreateTable(
                name: "CigarsEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Length = table.Column<string>(nullable: true),
                    Ring = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    Filler = table.Column<string>(nullable: true),
                    Wrapper = table.Column<string>(nullable: true),
                    Color = table.Column<string>(nullable: true),
                    Strength = table.Column<string>(nullable: true),
                    Shape = table.Column<string>(nullable: true),
                    Rating = table.Column<int>(nullable: false),
                    Price = table.Column<int>(nullable: false),
                    Brand = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CigarsEF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MoviesEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Season = table.Column<int>(nullable: false),
                    Episode = table.Column<int>(nullable: false),
                    StopPlayHour = table.Column<int>(nullable: false),
                    StopPlayMinute = table.Column<int>(nullable: false),
                    StopPlaySecond = table.Column<int>(nullable: false),
                    Archive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoviesEF", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CigarsEF");

            migrationBuilder.DropTable(
                name: "MoviesEF");

            migrationBuilder.CreateTable(
                name: "Cigars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Brand = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Color = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Country = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Filler = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Length = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Price = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Ring = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Shape = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Strength = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Wrapper = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cigars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Archive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Episode = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Season = table.Column<int>(type: "int", nullable: false),
                    StopPlayHour = table.Column<int>(type: "int", nullable: false),
                    StopPlayMinute = table.Column<int>(type: "int", nullable: false),
                    StopPlaySecond = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });
        }
    }
}
