using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTPilotFix02 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stats",
                table: "BtPilotEF");

            migrationBuilder.AddColumn<int>(
                name: "Gunnery",
                table: "BtPilotEF",
                maxLength: 8,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Piloting",
                table: "BtPilotEF",
                maxLength: 8,
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gunnery",
                table: "BtPilotEF");

            migrationBuilder.DropColumn(
                name: "Piloting",
                table: "BtPilotEF");

            migrationBuilder.AddColumn<string>(
                name: "Stats",
                table: "BtPilotEF",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
