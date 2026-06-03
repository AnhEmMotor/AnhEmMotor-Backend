using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Infrastructure.DBContexts;

#nullable disable

namespace Infrastructure.Migrations;

[Migration("20260601000001_AddSupplierContractNewColumns")]
public partial class AddSupplierContractNewColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "CreditLimit",
            table: "SupplierContracts",
            type: "decimal(18,2)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PaymentWindowDays",
            table: "SupplierContracts",
            type: "int",
            nullable: true);

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

        migrationBuilder.AddColumn<int>(
            name: "MinimumVolumePerMonth",
            table: "SupplierContracts",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "DiscountRate",
            table: "SupplierContracts",
            type: "decimal(5,2)",
            precision: 5,
            scale: 2,
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ParentContractId",
            table: "SupplierContracts",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "SupplierContractItem",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SupplierContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductVariantId = table.Column<int>(type: "int", nullable: false),
                WholesalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SupplierContractItem", x => x.Id);
                table.ForeignKey(
                    name: "FK_SupplierContractItem_SupplierContracts_SupplierContractId",
                    column: x => x.SupplierContractId,
                    principalTable: "SupplierContracts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SupplierContractItem_ProductVariants_ProductVariantId",
                    column: x => x.ProductVariantId,
                    principalTable: "ProductVariants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SupplierContractItem_SupplierContractId",
            table: "SupplierContractItem",
            column: "SupplierContractId");

        migrationBuilder.CreateIndex(
            name: "IX_SupplierContractItem_ProductVariantId",
            table: "SupplierContractItem",
            column: "ProductVariantId");

        migrationBuilder.CreateIndex(
            name: "IX_SupplierContracts_ParentContractId",
            table: "SupplierContracts",
            column: "ParentContractId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SupplierContractItem");

        migrationBuilder.DropIndex(
            name: "IX_SupplierContracts_ParentContractId",
            table: "SupplierContracts");

        migrationBuilder.DropColumn(
            name: "CreditLimit",
            table: "SupplierContracts");

        migrationBuilder.DropColumn(
            name: "PaymentWindowDays",
            table: "SupplierContracts");

        migrationBuilder.DropColumn(
            name: "BankAccountNumber",
            table: "SupplierContracts");

        migrationBuilder.DropColumn(
            name: "BankName",
            table: "SupplierContracts");

        migrationBuilder.DropColumn(
            name: "MinimumVolumePerMonth",
            table: "SupplierContracts");

        migrationBuilder.DropColumn(
            name: "DiscountRate",
            table: "SupplierContracts");

        migrationBuilder.DropColumn(
            name: "ParentContractId",
            table: "SupplierContracts");
    }
}
