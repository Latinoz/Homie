using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTMechModelModifImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "imageId",
                table: "BTMechsEF",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BTMechsEF_imageId",
                table: "BTMechsEF",
                column: "imageId");

            migrationBuilder.AddForeignKey(
                name: "FK_BTMechsEF_Picture_imageId",
                table: "BTMechsEF",
                column: "imageId",
                principalTable: "Picture",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BTMechsEF_Picture_imageId",
                table: "BTMechsEF");

            migrationBuilder.DropIndex(
                name: "IX_BTMechsEF_imageId",
                table: "BTMechsEF");

            migrationBuilder.DropColumn(
                name: "imageId",
                table: "BTMechsEF");
        }
    }
}
