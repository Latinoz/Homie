using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTModelsModify : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "ImageBTMechId",
                table: "BTMechsEF",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BTMechsEF_ImageBTMechId",
                table: "BTMechsEF",
                column: "ImageBTMechId");

            migrationBuilder.AddForeignKey(
                name: "FK_BTMechsEF_Picture_ImageBTMechId",
                table: "BTMechsEF",
                column: "ImageBTMechId",
                principalTable: "Picture",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BTMechsEF_Picture_ImageBTMechId",
                table: "BTMechsEF");

            migrationBuilder.DropIndex(
                name: "IX_BTMechsEF_ImageBTMechId",
                table: "BTMechsEF");

            migrationBuilder.DropColumn(
                name: "ImageBTMechId",
                table: "BTMechsEF");

            migrationBuilder.AddColumn<int>(
                name: "imageId",
                table: "BTMechsEF",
                type: "int",
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
    }
}
