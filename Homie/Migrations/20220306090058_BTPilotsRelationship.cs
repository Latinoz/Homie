using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTPilotsRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BtEF_BtPilotEF_BTPilotsModelId",
                table: "BtEF");

            migrationBuilder.DropIndex(
                name: "IX_BtEF_BTPilotsModelId",
                table: "BtEF");
        }
    }
}
