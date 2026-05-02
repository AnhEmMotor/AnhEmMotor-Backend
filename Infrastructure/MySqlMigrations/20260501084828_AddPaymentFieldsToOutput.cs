using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class AddPaymentFieldsToOutput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Output",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PaidAt",
                table: "Output",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Output",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Output",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Output",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Output");
        }
    }
}
