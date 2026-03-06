using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Homie.Migrations
{
    /// <inheritdoc />
    public partial class FinancesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(10)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CbrCode = table.Column<string>(type: "varchar(20)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsBase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Inflation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    CpiPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AccumulatedYearPercent = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inflation", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OperationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PreciousMetals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Metal = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Purity = table.Column<string>(type: "varchar(10)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WeightGrams = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CurrentPricePerGram = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAutoUpdateEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastManualOverrideDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreciousMetals", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRates_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FinanceAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BankName = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceAccounts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Instruments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Exchange = table.Column<int>(type: "int", nullable: false),
                    ISIN = table.Column<string>(type: "varchar(20)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalCode = table.Column<string>(type: "varchar(50)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "varchar(100)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastPrice = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    LastPriceDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastPriceSource = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instruments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Instruments_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Deposits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    OpenDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsCapitalization = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deposits_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Deposits_FinanceAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "FinanceAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CryptoAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: true),
                    Ticker = table.Column<string>(type: "varchar(20)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoinGeckoId = table.Column<string>(type: "varchar(50)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    AvgPurchasePrice = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsAutoUpdateEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastManualOverrideDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WalletAddress = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CryptoAssets_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CryptoAssets_FinanceAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "FinanceAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CryptoAssets_Instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FinanceOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    OperationTypeId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    InstrumentId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    ExchangeRateToRub = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    AmountInRub = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceOperations_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinanceOperations_FinanceAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "FinanceAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinanceOperations_Instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinanceOperations_OperationTypes_OperationTypeId",
                        column: x => x.OperationTypeId,
                        principalTable: "OperationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InvestmentPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    AvgPurchasePrice = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsAutoUpdateEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastManualOverrideDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentPositions_FinanceAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "FinanceAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestmentPositions_Instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PriceHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    PriceInRub = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceHistory_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriceHistory_Instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "OperationTypes",
                columns: new[] { "Id", "Category", "Name", "UserUid" },
                values: new object[,]
                {
                    { 1, 0, "Пополнение депозита", null },
                    { 2, 0, "Снятие с депозита", null },
                    { 3, 0, "Начисление процентов", null },
                    { 4, 1, "Покупка ценных бумаг", null },
                    { 5, 1, "Продажа ценных бумаг", null },
                    { 6, 1, "Дивиденд", null },
                    { 7, 1, "Купон", null },
                    { 8, 1, "Комиссия", null },
                    { 9, 1, "Налог", null },
                    { 10, 2, "Покупка криптовалюты", null },
                    { 11, 2, "Продажа криптовалюты", null },
                    { 12, 3, "Покупка драгметалла", null },
                    { 13, 3, "Продажа драгметалла", null },
                    { 14, 4, "Перевод", null },
                    { 15, 5, "Прочее", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAssets_AccountId",
                table: "CryptoAssets",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAssets_CurrencyId",
                table: "CryptoAssets",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAssets_InstrumentId",
                table: "CryptoAssets",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAssets_UserUid",
                table: "CryptoAssets",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_UserUid",
                table: "Currencies",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_AccountId",
                table: "Deposits",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_CurrencyId",
                table: "Deposits",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_UserUid",
                table: "Deposits",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_CurrencyId_Date_UserUid",
                table: "ExchangeRates",
                columns: new[] { "CurrencyId", "Date", "UserUid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_UserUid",
                table: "ExchangeRates",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceAccounts_CurrencyId",
                table: "FinanceAccounts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceAccounts_UserUid",
                table: "FinanceAccounts",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceOperations_AccountId",
                table: "FinanceOperations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceOperations_CurrencyId",
                table: "FinanceOperations",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceOperations_Date_UserUid",
                table: "FinanceOperations",
                columns: new[] { "Date", "UserUid" });

            migrationBuilder.CreateIndex(
                name: "IX_FinanceOperations_InstrumentId",
                table: "FinanceOperations",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceOperations_OperationTypeId",
                table: "FinanceOperations",
                column: "OperationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceOperations_UserUid",
                table: "FinanceOperations",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_Inflation_Year_Month_UserUid",
                table: "Inflation",
                columns: new[] { "Year", "Month", "UserUid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Code_UserUid",
                table: "Instruments",
                columns: new[] { "Code", "UserUid" });

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_CurrencyId",
                table: "Instruments",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_UserUid",
                table: "Instruments",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentPositions_AccountId",
                table: "InvestmentPositions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentPositions_InstrumentId",
                table: "InvestmentPositions",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentPositions_UserUid",
                table: "InvestmentPositions",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_OperationTypes_UserUid",
                table: "OperationTypes",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_PreciousMetals_UserUid",
                table: "PreciousMetals",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_CurrencyId",
                table: "PriceHistory",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_InstrumentId_Date",
                table: "PriceHistory",
                columns: new[] { "InstrumentId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_InstrumentId_Date_UserUid",
                table: "PriceHistory",
                columns: new[] { "InstrumentId", "Date", "UserUid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CryptoAssets");

            migrationBuilder.DropTable(
                name: "Deposits");

            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "FinanceOperations");

            migrationBuilder.DropTable(
                name: "Inflation");

            migrationBuilder.DropTable(
                name: "InvestmentPositions");

            migrationBuilder.DropTable(
                name: "PreciousMetals");

            migrationBuilder.DropTable(
                name: "PriceHistory");

            migrationBuilder.DropTable(
                name: "OperationTypes");

            migrationBuilder.DropTable(
                name: "FinanceAccounts");

            migrationBuilder.DropTable(
                name: "Instruments");

            migrationBuilder.DropTable(
                name: "Currencies");
        }
    }
}
