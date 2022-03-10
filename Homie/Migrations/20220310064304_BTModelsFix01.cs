using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTModelsFix01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropColumn(
                name: "BTMechsModelId",
                table: "BtPilotEF");

            migrationBuilder.AddColumn<int>(
                name: "BTPilotsModelId",
                table: "BtEF",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BtEF_BTPilotsModelId",
                table: "BtEF",
                column: "BTPilotsModelId",
                unique: true);

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

            migrationBuilder.DropIndex(
                name: "IX_BtEF_BTPilotsModelId",
                table: "BtEF");

            migrationBuilder.DropColumn(
                name: "BTPilotsModelId",
                table: "BtEF");

            migrationBuilder.AddColumn<int>(
                name: "BTMechsModelId",
                table: "BtPilotEF",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BtPilotEF_BTMechsModelId",
                table: "BtPilotEF",
                column: "BTMechsModelId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BtPilotEF_BtEF_BTMechsModelId",
                table: "BtPilotEF",
                column: "BTMechsModelId",
                principalTable: "BtEF",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
