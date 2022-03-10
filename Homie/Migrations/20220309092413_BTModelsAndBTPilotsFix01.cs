using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTModelsAndBTPilotsFix01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.CreateIndex(
                name: "IX_BtEF_BTPilotsModelId",
                table: "BtEF",
                column: "BTPilotsModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_BtEF_BtPilotEF_BTPilotsModelId",
                table: "BtEF",
                column: "BTPilotsModelId",
                principalTable: "BtPilotEF",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
