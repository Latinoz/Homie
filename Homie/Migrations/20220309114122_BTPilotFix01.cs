using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTPilotFix01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeMech",
                table: "BtPilotEF");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TypeMech",
                table: "BtPilotEF",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
