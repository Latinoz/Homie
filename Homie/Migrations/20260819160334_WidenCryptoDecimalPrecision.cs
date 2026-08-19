using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homie.Migrations
{
    /// <inheritdoc />
    public partial class WidenCryptoDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "FinanceOperations",
                type: "decimal(28,12)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,12)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "FinanceOperations",
                type: "decimal(28,12)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,12)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "CryptoAssets",
                type: "decimal(28,12)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,12)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentPrice",
                table: "CryptoAssets",
                type: "decimal(28,12)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,12)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AvgPurchasePrice",
                table: "CryptoAssets",
                type: "decimal(28,12)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,12)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "FinanceOperations",
                type: "decimal(18,12)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,12)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "FinanceOperations",
                type: "decimal(18,12)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,12)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "CryptoAssets",
                type: "decimal(18,12)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,12)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentPrice",
                table: "CryptoAssets",
                type: "decimal(18,12)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,12)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AvgPurchasePrice",
                table: "CryptoAssets",
                type: "decimal(18,12)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,12)");
        }
    }
}
