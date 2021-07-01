using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTMechModelMod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BTMechsEF_Picture_ImageBTMechId",
                table: "BTMechsEF");

            migrationBuilder.DropIndex(
                name: "IX_BTMechsEF_ImageBTMechId",
                table: "BTMechsEF");

            migrationBuilder.DropColumn(
                name: "Game",
                table: "BTMechsEF");

            migrationBuilder.DropColumn(
                name: "ImageBTMechId",
                table: "BTMechsEF");

            migrationBuilder.AddColumn<int>(
                name: "GameType",
                table: "BTMechsEF",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImgMech",
                table: "BTMechsEF",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameType",
                table: "BTMechsEF");

            migrationBuilder.DropColumn(
                name: "ImgMech",
                table: "BTMechsEF");

            migrationBuilder.AddColumn<int>(
                name: "Game",
                table: "BTMechsEF",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ImageBTMechId",
                table: "BTMechsEF",
                type: "int",
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
    }
}
