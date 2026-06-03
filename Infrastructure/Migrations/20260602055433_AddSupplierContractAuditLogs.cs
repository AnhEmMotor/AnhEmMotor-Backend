using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierContractAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SupplierContracts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "SupplierContracts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "SupplierContracts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "SupplierContracts",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate",
                table: "SupplierContracts",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumVolumePerMonth",
                table: "SupplierContracts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentContractId",
                table: "SupplierContracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentWindowDays",
                table: "SupplierContracts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplierContractAuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierContractAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierContractAuditLog_SupplierContracts_SupplierContractId",
                        column: x => x.SupplierContractId,
                        principalTable: "SupplierContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierContractItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    WholesalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierContractItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierContractItem_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierContractItem_SupplierContracts_SupplierContractId",
                        column: x => x.SupplierContractId,
                        principalTable: "SupplierContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierDebtSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDebtSettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDebtSettlements_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierFinances",
                columns: table => new
                {
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    CurrentDebt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierFinances", x => x.SupplierId);
                    table.ForeignKey(
                        name: "FK_SupplierFinances_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContracts_ParentContractId",
                table: "SupplierContracts",
                column: "ParentContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContractAuditLog_SupplierContractId",
                table: "SupplierContractAuditLog",
                column: "SupplierContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContractItem_ProductVariantId",
                table: "SupplierContractItem",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContractItem_SupplierContractId",
                table: "SupplierContractItem",
                column: "SupplierContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebtSettlements_SupplierId",
                table: "SupplierDebtSettlements",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierContracts_SupplierContracts_ParentContractId",
                table: "SupplierContracts",
                column: "ParentContractId",
                principalTable: "SupplierContracts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplierContracts_SupplierContracts_ParentContractId",
                table: "SupplierContracts");

            migrationBuilder.DropTable(
                name: "SupplierContractAuditLog");

            migrationBuilder.DropTable(
                name: "SupplierContractItem");

            migrationBuilder.DropTable(
                name: "SupplierDebtSettlements");

            migrationBuilder.DropTable(
                name: "SupplierFinances");

            migrationBuilder.DropIndex(
                name: "IX_SupplierContracts_ParentContractId",
                table: "SupplierContracts");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "SupplierContracts");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "SupplierContracts");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "SupplierContracts");

            migrationBuilder.DropColumn(
                name: "DiscountRate",
                table: "SupplierContracts");

            migrationBuilder.DropColumn(
                name: "MinimumVolumePerMonth",
                table: "SupplierContracts");

            migrationBuilder.DropColumn(
                name: "ParentContractId",
                table: "SupplierContracts");

            migrationBuilder.DropColumn(
                name: "PaymentWindowDays",
                table: "SupplierContracts");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SupplierContracts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
