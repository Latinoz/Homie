using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Homie.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Instruments",
                newName: "InvestmentTypeId");

            migrationBuilder.CreateTable(
                name: "InvestmentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SystemCode = table.Column<string>(type: "varchar(50)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "InvestmentTypes",
                columns: new[] { "Id", "Name", "SystemCode", "UserUid" },
                values: new object[,]
                {
                    { 1, "Акции", "Stock", null },
                    { 2, "Облигации", "Bond", null },
                    { 3, "ETF", "ETF", null },
                    { 4, "Криптовалюта", "Crypto", null },
                    { 5, "Драгметаллы", "PreciousMetal", null },
                    { 6, "Валюта", "Currency", null }
                });

            // Миграция данных: старый enum (0=Stock,1=Bond,2=ETF,3=Crypto,4=PreciousMetal) → новые FK Id (+1)
            migrationBuilder.Sql("UPDATE Instruments SET InvestmentTypeId = InvestmentTypeId + 1;");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_InvestmentTypeId",
                table: "Instruments",
                column: "InvestmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentTypes_UserUid",
                table: "InvestmentTypes",
                column: "UserUid");

            migrationBuilder.AddForeignKey(
                name: "FK_Instruments_InvestmentTypes_InvestmentTypeId",
                table: "Instruments",
                column: "InvestmentTypeId",
                principalTable: "InvestmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instruments_InvestmentTypes_InvestmentTypeId",
                table: "Instruments");

            migrationBuilder.DropTable(
                name: "InvestmentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Instruments_InvestmentTypeId",
                table: "Instruments");

            migrationBuilder.RenameColumn(
                name: "InvestmentTypeId",
                table: "Instruments",
                newName: "Type");
        }
    }
}
