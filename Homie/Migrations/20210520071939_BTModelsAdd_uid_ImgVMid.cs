using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTModelsAdd_uid_ImgVMid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "_uid",
                table: "Picture",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "ImgVMidId",
                table: "BTMechsEF",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BTMechsEF_ImgVMidId",
                table: "BTMechsEF",
                column: "ImgVMidId");

            migrationBuilder.AddForeignKey(
                name: "FK_BTMechsEF_Picture_ImgVMidId",
                table: "BTMechsEF",
                column: "ImgVMidId",
                principalTable: "Picture",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BTMechsEF_Picture_ImgVMidId",
                table: "BTMechsEF");

            migrationBuilder.DropIndex(
                name: "IX_BTMechsEF_ImgVMidId",
                table: "BTMechsEF");

            migrationBuilder.DropColumn(
                name: "_uid",
                table: "Picture");

            migrationBuilder.DropColumn(
                name: "ImgVMidId",
                table: "BTMechsEF");
        }
    }
}
