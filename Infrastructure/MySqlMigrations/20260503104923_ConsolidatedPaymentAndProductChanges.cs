using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class ConsolidatedPaymentAndProductChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxPurchaseQuantity",
                table: "ProductCategory",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<int>(name: "DepositRatio", table: "Output", type: "int", nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Output",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<long>(name: "PaidAt", table: "Output", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<string>(name: "PaymentCode", table: "Output", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<long>(name: "PaymentExpiredAt", table: "Output", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<string>(name: "PaymentMethod", table: "Output", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(name: "PaymentStatus", table: "Output", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(name: "PaymentUrl", table: "Output", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(name: "TransactionId", table: "Output", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "MaxPurchaseQuantity", table: "ProductCategory");
            migrationBuilder.DropColumn(name: "DepositRatio", table: "Output");
            migrationBuilder.DropColumn(name: "PaidAmount", table: "Output");
            migrationBuilder.DropColumn(name: "PaidAt", table: "Output");
            migrationBuilder.DropColumn(name: "PaymentCode", table: "Output");
            migrationBuilder.DropColumn(name: "PaymentExpiredAt", table: "Output");
            migrationBuilder.DropColumn(name: "PaymentMethod", table: "Output");
            migrationBuilder.DropColumn(name: "PaymentStatus", table: "Output");
            migrationBuilder.DropColumn(name: "PaymentUrl", table: "Output");
            migrationBuilder.DropColumn(name: "TransactionId", table: "Output");
        }
    }
}
