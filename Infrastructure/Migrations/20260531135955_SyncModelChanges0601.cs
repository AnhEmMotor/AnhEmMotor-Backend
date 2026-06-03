using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges0601 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplierContracts_Supplier_SupplierId",
                table: "SupplierContracts");

            migrationBuilder.DropIndex(
                name: "IX_SupplierContracts_ContractNumber",
                table: "SupplierContracts");

            // migrationBuilder.DropUniqueConstraint(
            //    name: "AK_Supplier_TempId",
            //    table: "Supplier");

            // migrationBuilder.DropColumn(
            //    name: "TempId",
            //    table: "Supplier");

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "SupplierContracts",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SupplierContracts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldDefaultValue: "Draft");



            migrationBuilder.CreateTable(
                name: "ContractTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DynamicFields = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinanceContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoanAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DisbursementStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CavetLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OutputId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShowroomName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowroomTaxCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowroomAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowroomRepresentative = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerCCCD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FrameNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualSalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalPaymentDeadline = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    WarrantyPeriod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarrantyScope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecialTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ScannedFileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesContracts_Output_OutputId",
                        column: x => x.OutputId,
                        principalTable: "Output",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_SalesContracts_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            // migrationBuilder.InsertData(
            //     table: "PartnerType",
            //     columns: new[] { "Key", "CreatedAt", "DeletedAt", "UpdatedAt" },
            //     values: new object[,]
            //     {
            //         { "financial", null, null, null },
            //         { "insurance", null, null, null },
            //         { "supplier", null, null, null }
            //     });

            // migrationBuilder.InsertData(
            //     table: "ProductStatus",
            //     columns: new[] { "Key", "CreatedAt", "DeletedAt", "UpdatedAt" },
            //     values: new object[,]
            //     {
            //         { "for-sale", null, null, null },
            //         { "out-of-business", null, null, null }
            //     });

            migrationBuilder.CreateIndex(
                name: "IX_SalesContracts_CustomerId",
                table: "SalesContracts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesContracts_OutputId",
                table: "SalesContracts",
                column: "OutputId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierContracts_Supplier_SupplierId",
                table: "SupplierContracts",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplierContracts_Supplier_SupplierId",
                table: "SupplierContracts");

            migrationBuilder.DropTable(
                name: "ContractTemplates");

            migrationBuilder.DropTable(
                name: "FinanceContracts");

            migrationBuilder.DropTable(
                name: "SalesContracts");

            migrationBuilder.DeleteData(
                table: "PartnerType",
                keyColumn: "Key",
                keyValue: "financial");

            migrationBuilder.DeleteData(
                table: "PartnerType",
                keyColumn: "Key",
                keyValue: "insurance");

            migrationBuilder.DeleteData(
                table: "PartnerType",
                keyColumn: "Key",
                keyValue: "supplier");

            migrationBuilder.DeleteData(
                table: "ProductStatus",
                keyColumn: "Key",
                keyValue: "for-sale");

            migrationBuilder.DeleteData(
                table: "ProductStatus",
                keyColumn: "Key",
                keyValue: "out-of-business");

            migrationBuilder.AlterColumn<Guid>(
                name: "SupplierId",
                table: "SupplierContracts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SupplierContracts",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");



            // migrationBuilder.AddColumn<Guid>(
            //    name: "TempId",
            //    table: "Supplier",
            //    type: "uniqueidentifier",
            //    nullable: false,
            //    defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // migrationBuilder.AddUniqueConstraint(
            //    name: "AK_Supplier_TempId",
            //    table: "Supplier",
            //    column: "TempId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContracts_ContractNumber",
                table: "SupplierContracts",
                column: "ContractNumber",
                unique: true);

            // migrationBuilder.AddForeignKey(
            //    name: "FK_SupplierContracts_Supplier_SupplierId",
            //    table: "SupplierContracts",
            //    column: "SupplierId",
            //    principalTable: "Supplier",
            //    principalColumn: "TempId",
            //    onDelete: ReferentialAction.Restrict);
        }
    }
}

