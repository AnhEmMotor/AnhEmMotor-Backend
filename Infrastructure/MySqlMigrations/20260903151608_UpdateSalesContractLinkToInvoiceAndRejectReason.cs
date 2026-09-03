using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class UpdateSalesContractLinkToInvoiceAndRejectReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesContracts_Output_OutputId",
                table: "SalesContracts");

            migrationBuilder.RenameColumn(
                name: "OutputId",
                table: "SalesContracts",
                newName: "InvoiceId");

            migrationBuilder.RenameIndex(
                name: "IX_SalesContracts_OutputId",
                table: "SalesContracts",
                newName: "IX_SalesContracts_InvoiceId");

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "SalesContracts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DepositPercentage",
                table: "Invoice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherCode",
                table: "Invoice",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesContracts_Invoice_InvoiceId",
                table: "SalesContracts",
                column: "InvoiceId",
                principalTable: "Invoice",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesContracts_Invoice_InvoiceId",
                table: "SalesContracts");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "SalesContracts");

            migrationBuilder.DropColumn(
                name: "DepositPercentage",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "VoucherCode",
                table: "Invoice");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "SalesContracts",
                newName: "OutputId");

            migrationBuilder.RenameIndex(
                name: "IX_SalesContracts_InvoiceId",
                table: "SalesContracts",
                newName: "IX_SalesContracts_OutputId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesContracts_Output_OutputId",
                table: "SalesContracts",
                column: "OutputId",
                principalTable: "Output",
                principalColumn: "id");
        }
    }
}
