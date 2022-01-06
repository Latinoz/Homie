using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTModelsDelFromBT : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BTBattleArmourInfantryEF");

            migrationBuilder.DropTable(
                name: "BTVehiclesEF");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BTMechsEF",
                table: "BTMechsEF");

            migrationBuilder.RenameTable(
                name: "BTMechsEF",
                newName: "BtEF");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BtEF",
                table: "BtEF",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BtEF",
                table: "BtEF");

            migrationBuilder.RenameTable(
                name: "BtEF",
                newName: "BTMechsEF");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BTMechsEF",
                table: "BTMechsEF",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BTBattleArmourInfantryEF",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Bv = table.Column<int>(type: "int", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    Game = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Return = table.Column<int>(type: "int", nullable: false),
                    Sent = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Tonnage = table.Column<int>(type: "int", nullable: false),
                    TypeArmor = table.Column<int>(type: "int", nullable: false),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTBattleArmourInfantryEF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BTVehiclesEF",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Bv = table.Column<int>(type: "int", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    Game = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    PilotUid = table.Column<string>(type: "varchar(255)", nullable: true),
                    Return = table.Column<int>(type: "int", nullable: false),
                    Sent = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Tonnage = table.Column<int>(type: "int", nullable: false),
                    TypeVehicles = table.Column<int>(type: "int", nullable: false),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTVehiclesEF", x => x.Id);
                });
        }
    }
}
