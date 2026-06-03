using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Infrastructure.DBContexts;

#nullable disable

namespace Infrastructure.Migrations;

[Migration("20260601000001_AddSupplierContractCreditAndPriceColumns")]
public partial class AddSupplierContractCreditAndPriceColumns : Migration
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

        migrationBuilder.CreateIndex(
            name: "IX_SupplierContracts_ParentContractId",
            table: "SupplierContracts",
            column: "ParentContractId");

        migrationBuilder.AddForeignKey(
            name: "FK_SupplierContracts_SupplierContracts_ParentContractId",
            table: "SupplierContracts",
            column: "ParentContractId",
            principalTable: "SupplierContracts",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_SupplierContracts_SupplierContracts_ParentContractId",
            table: "SupplierContracts");

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
