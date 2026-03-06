using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class ModifyModelsBT : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Game",
                table: "BTVehiclesEF",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Game",
                table: "BTMechsEF",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Game",
                table: "BTBattleArmourInfantryEF",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Game",
                table: "BTVehiclesEF");

            migrationBuilder.DropColumn(
                name: "Game",
                table: "BTMechsEF");

            migrationBuilder.DropColumn(
                name: "Game",
                table: "BTBattleArmourInfantryEF");
        }
    }
}
