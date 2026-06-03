using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Infrastructure.DBContexts;

#nullable disable

namespace Infrastructure.Migrations
{
[DbContext(typeof(ApplicationDBContext))]
[Migration("20260530000000_AddSupplierContractEntity")]
public partial class AddSupplierContractEntity : Migration
{
protected override void Up(MigrationBuilder migrationBuilder)
{
migrationBuilder.CreateTable(
name: "SupplierContracts",
columns: table => new
{
Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
SupplierId = table.Column<int>(type: "int", nullable: true),
ContractNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
ContractFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
ContractValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
Terms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
},
constraints: table =>
{
table.PrimaryKey("PK_SupplierContracts", x => x.Id);
table.ForeignKey(
name: "FK_SupplierContracts_Supplier_SupplierId",
column: x => x.SupplierId,
principalTable: "Supplier",
principalColumn: "Id",
onDelete: ReferentialAction.Restrict);
});

migrationBuilder.CreateIndex(
name: "IX_SupplierContracts_ContractNumber",
table: "SupplierContracts",
column: "ContractNumber",
unique: true);

migrationBuilder.CreateIndex(
name: "IX_SupplierContracts_SupplierId",
table: "SupplierContracts",
column: "SupplierId");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
migrationBuilder.DropTable(
name: "SupplierContracts");
}
}
}
