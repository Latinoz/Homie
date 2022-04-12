using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTPilotsAndMechsGuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PilotUid",
                table: "BtPilotEF",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MechUid",
                table: "BtEF",
                type: "varchar(255)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PilotUid",
                table: "BtPilotEF");

            migrationBuilder.DropColumn(
                name: "MechUid",
                table: "BtEF");
        }
    }
}
