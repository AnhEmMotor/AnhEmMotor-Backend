using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
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
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(name: "DepositRatio", table: "Output", type: "integer", nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Output",
                type: "numeric(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaidAt",
                table: "Output",
                type: "timestamp with time zone",
                nullable: true);
            migrationBuilder.AddColumn<string>(name: "PaymentCode", table: "Output", type: "text", nullable: true);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaymentExpiredAt",
                table: "Output",
                type: "timestamp with time zone",
                nullable: true);
            migrationBuilder.AddColumn<string>(name: "PaymentMethod", table: "Output", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "PaymentStatus", table: "Output", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "PaymentUrl", table: "Output", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(name: "TransactionId", table: "Output", type: "text", nullable: true);
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
