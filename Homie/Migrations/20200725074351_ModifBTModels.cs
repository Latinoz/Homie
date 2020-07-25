using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class ModifBTModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BTBattleArmourInfantryEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Tonnage = table.Column<int>(nullable: false),
                    Experience = table.Column<int>(nullable: false),
                    Bv = table.Column<int>(nullable: false),
                    Sent = table.Column<int>(nullable: false),
                    Return = table.Column<int>(nullable: false),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTBattleArmourInfantryEF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BTMechsEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Tonnage = table.Column<int>(nullable: false),
                    Experience = table.Column<int>(nullable: false),
                    Bv = table.Column<int>(nullable: false),
                    Sent = table.Column<int>(nullable: false),
                    Return = table.Column<int>(nullable: false),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true),
                    PilotUid = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTMechsEF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BTVehiclesEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Tonnage = table.Column<int>(nullable: false),
                    Experience = table.Column<int>(nullable: false),
                    Bv = table.Column<int>(nullable: false),
                    Sent = table.Column<int>(nullable: false),
                    Return = table.Column<int>(nullable: false),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true),
                    PilotUid = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTVehiclesEF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BTBattleArmourInfantryStateEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(nullable: true),
                    BTArmourInfantryModelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTBattleArmourInfantryStateEF", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BTBattleArmourInfantryStateEF_BTBattleArmourInfantryEF_BTArm~",
                        column: x => x.BTArmourInfantryModelId,
                        principalTable: "BTBattleArmourInfantryEF",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BTBattleArmourInfantryTypeEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(nullable: true),
                    BTArmourInfantryModelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTBattleArmourInfantryTypeEF", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BTBattleArmourInfantryTypeEF_BTBattleArmourInfantryEF_BTArmo~",
                        column: x => x.BTArmourInfantryModelId,
                        principalTable: "BTBattleArmourInfantryEF",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BTMechStateEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(nullable: true),
                    BTMechsModelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTMechStateEF", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BTMechStateEF_BTMechsEF_BTMechsModelId",
                        column: x => x.BTMechsModelId,
                        principalTable: "BTMechsEF",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BTMechTypeEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(nullable: true),
                    BTMechsModelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTMechTypeEF", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BTMechTypeEF_BTMechsEF_BTMechsModelId",
                        column: x => x.BTMechsModelId,
                        principalTable: "BTMechsEF",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BTVehicleStateEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(nullable: true),
                    BTVehiclesModelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTVehicleStateEF", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BTVehicleStateEF_BTVehiclesEF_BTVehiclesModelId",
                        column: x => x.BTVehiclesModelId,
                        principalTable: "BTVehiclesEF",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BTVehicleTypeEF",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(nullable: true),
                    BTVehiclesModelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTVehicleTypeEF", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BTVehicleTypeEF_BTVehiclesEF_BTVehiclesModelId",
                        column: x => x.BTVehiclesModelId,
                        principalTable: "BTVehiclesEF",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BTBattleArmourInfantryStateEF_BTArmourInfantryModelId",
                table: "BTBattleArmourInfantryStateEF",
                column: "BTArmourInfantryModelId");

            migrationBuilder.CreateIndex(
                name: "IX_BTBattleArmourInfantryTypeEF_BTArmourInfantryModelId",
                table: "BTBattleArmourInfantryTypeEF",
                column: "BTArmourInfantryModelId");

            migrationBuilder.CreateIndex(
                name: "IX_BTMechStateEF_BTMechsModelId",
                table: "BTMechStateEF",
                column: "BTMechsModelId");

            migrationBuilder.CreateIndex(
                name: "IX_BTMechTypeEF_BTMechsModelId",
                table: "BTMechTypeEF",
                column: "BTMechsModelId");

            migrationBuilder.CreateIndex(
                name: "IX_BTVehicleStateEF_BTVehiclesModelId",
                table: "BTVehicleStateEF",
                column: "BTVehiclesModelId");

            migrationBuilder.CreateIndex(
                name: "IX_BTVehicleTypeEF_BTVehiclesModelId",
                table: "BTVehicleTypeEF",
                column: "BTVehiclesModelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BTBattleArmourInfantryStateEF");

            migrationBuilder.DropTable(
                name: "BTBattleArmourInfantryTypeEF");

            migrationBuilder.DropTable(
                name: "BTMechStateEF");

            migrationBuilder.DropTable(
                name: "BTMechTypeEF");

            migrationBuilder.DropTable(
                name: "BTVehicleStateEF");

            migrationBuilder.DropTable(
                name: "BTVehicleTypeEF");

            migrationBuilder.DropTable(
                name: "BTBattleArmourInfantryEF");

            migrationBuilder.DropTable(
                name: "BTMechsEF");

            migrationBuilder.DropTable(
                name: "BTVehiclesEF");
        }
    }
}
