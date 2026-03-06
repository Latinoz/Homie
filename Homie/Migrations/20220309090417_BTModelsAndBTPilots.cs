using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTModelsAndBTPilots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BtEF_BtPilotEF_BTPilotsModelId",
                table: "BtEF");

            migrationBuilder.AddColumn<int>(
                name: "BTMechsModelId",
                table: "BtPilotEF",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "BTPilotsModelId",
                table: "BtEF",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_BtEF_BtPilotEF_BTPilotsModelId",
                table: "BtEF",
                column: "BTPilotsModelId",
                principalTable: "BtPilotEF",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BtEF_BtPilotEF_BTPilotsModelId",
                table: "BtEF");

            migrationBuilder.DropColumn(
                name: "BTMechsModelId",
                table: "BtPilotEF");

            migrationBuilder.AlterColumn<int>(
                name: "BTPilotsModelId",
                table: "BtEF",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BtEF_BtPilotEF_BTPilotsModelId",
                table: "BtEF",
                column: "BTPilotsModelId",
                principalTable: "BtPilotEF",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
