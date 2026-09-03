using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
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
                type: "text",
                nullable: true);

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
