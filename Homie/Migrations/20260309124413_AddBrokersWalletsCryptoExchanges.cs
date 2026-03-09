using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homie.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokersWalletsCryptoExchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BrokerId",
                table: "FinanceAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CryptoExchangeId",
                table: "FinanceAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WalletId",
                table: "FinanceAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Brokers",
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
                    table.PrimaryKey("PK_Brokers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CryptoExchanges",
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
                    table.PrimaryKey("PK_CryptoExchanges", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceAccounts_BrokerId",
                table: "FinanceAccounts",
                column: "BrokerId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceAccounts_CryptoExchangeId",
                table: "FinanceAccounts",
                column: "CryptoExchangeId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceAccounts_WalletId",
                table: "FinanceAccounts",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Brokers_UserUid",
                table: "Brokers",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoExchanges_UserUid",
                table: "CryptoExchanges",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CurrencyId",
                table: "Wallets",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserUid",
                table: "Wallets",
                column: "UserUid");

            migrationBuilder.AddForeignKey(
                name: "FK_FinanceAccounts_Brokers_BrokerId",
                table: "FinanceAccounts",
                column: "BrokerId",
                principalTable: "Brokers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FinanceAccounts_CryptoExchanges_CryptoExchangeId",
                table: "FinanceAccounts",
                column: "CryptoExchangeId",
                principalTable: "CryptoExchanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FinanceAccounts_Wallets_WalletId",
                table: "FinanceAccounts",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinanceAccounts_Brokers_BrokerId",
                table: "FinanceAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_FinanceAccounts_CryptoExchanges_CryptoExchangeId",
                table: "FinanceAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_FinanceAccounts_Wallets_WalletId",
                table: "FinanceAccounts");

            migrationBuilder.DropTable(
                name: "Brokers");

            migrationBuilder.DropTable(
                name: "CryptoExchanges");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_FinanceAccounts_BrokerId",
                table: "FinanceAccounts");

            migrationBuilder.DropIndex(
                name: "IX_FinanceAccounts_CryptoExchangeId",
                table: "FinanceAccounts");

            migrationBuilder.DropIndex(
                name: "IX_FinanceAccounts_WalletId",
                table: "FinanceAccounts");

            migrationBuilder.DropColumn(
                name: "BrokerId",
                table: "FinanceAccounts");

            migrationBuilder.DropColumn(
                name: "CryptoExchangeId",
                table: "FinanceAccounts");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "FinanceAccounts");
        }
    }
}
