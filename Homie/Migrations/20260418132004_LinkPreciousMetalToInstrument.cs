using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homie.Migrations
{
    /// <inheritdoc />
    public partial class LinkPreciousMetalToInstrument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstrumentId",
                table: "PreciousMetals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreciousMetals_InstrumentId",
                table: "PreciousMetals",
                column: "InstrumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreciousMetals_Instruments_InstrumentId",
                table: "PreciousMetals",
                column: "InstrumentId",
                principalTable: "Instruments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreciousMetals_Instruments_InstrumentId",
                table: "PreciousMetals");

            migrationBuilder.DropIndex(
                name: "IX_PreciousMetals_InstrumentId",
                table: "PreciousMetals");

            migrationBuilder.DropColumn(
                name: "InstrumentId",
                table: "PreciousMetals");
        }
    }
}
