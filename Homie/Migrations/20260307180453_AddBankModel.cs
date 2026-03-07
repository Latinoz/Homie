using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homie.Migrations
{
    /// <inheritdoc />
    public partial class AddBankModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankName",
                table: "FinanceAccounts");

            migrationBuilder.AddColumn<int>(
                name: "BankId",
                table: "FinanceAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceAccounts_BankId",
                table: "FinanceAccounts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_UserUid",
                table: "Banks",
                column: "UserUid");

            migrationBuilder.AddForeignKey(
                name: "FK_FinanceAccounts_Banks_BankId",
                table: "FinanceAccounts",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinanceAccounts_Banks_BankId",
                table: "FinanceAccounts");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropIndex(
                name: "IX_FinanceAccounts_BankId",
                table: "FinanceAccounts");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "FinanceAccounts");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "FinanceAccounts",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
