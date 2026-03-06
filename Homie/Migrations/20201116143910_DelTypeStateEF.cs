using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Homie.Migrations
{
    public partial class DelTypeStateEF : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "BTVehiclesEF",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeVehicles",
                table: "BTVehiclesEF",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "BTMechsEF",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeMech",
                table: "BTMechsEF",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "BTBattleArmourInfantryEF",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeArmor",
                table: "BTBattleArmourInfantryEF",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "BTVehiclesEF");

            migrationBuilder.DropColumn(
                name: "TypeVehicles",
                table: "BTVehiclesEF");

            migrationBuilder.DropColumn(
                name: "State",
                table: "BTMechsEF");

            migrationBuilder.DropColumn(
                name: "TypeMech",
                table: "BTMechsEF");

            migrationBuilder.DropColumn(
                name: "State",
                table: "BTBattleArmourInfantryEF");

            migrationBuilder.DropColumn(
                name: "TypeArmor",
                table: "BTBattleArmourInfantryEF");

            migrationBuilder.CreateTable(
                name: "BTBattleArmourInfantryStateEF",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BTArmourInfantryModelId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BTArmourInfantryModelId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BTMechsModelId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BTMechsModelId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BTVehiclesModelId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BTVehiclesModelId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
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
    }
}
