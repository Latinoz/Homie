using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class BTPilots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PilotUid",
                table: "BtEF");

            migrationBuilder.AddColumn<int>(
                name: "BTPilotsModelId",
                table: "BtEF",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BtPilotEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Experience = table.Column<int>(nullable: false),
                    Sent = table.Column<int>(nullable: false),
                    Return = table.Column<int>(nullable: false),
                    Stats = table.Column<string>(nullable: true),
                    Raiting = table.Column<string>(nullable: true),
                    Hits = table.Column<int>(nullable: false),
                    TypeMech = table.Column<string>(nullable: true),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true),
                    MechId = table.Column<int>(nullable: false),
                    GameType = table.Column<int>(nullable: false),
                    ImgBT = table.Column<string>(type: "varchar(255)", nullable: true),
                    Avatar = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BtPilotEF", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BtPilotEF");

            migrationBuilder.DropColumn(
                name: "BTPilotsModelId",
                table: "BtEF");

            migrationBuilder.AddColumn<string>(
                name: "PilotUid",
                table: "BtEF",
                type: "varchar(255)",
                nullable: true);
        }
    }
}
