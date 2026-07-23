using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaForShippingVoucherAndWarranty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Dimensions", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "Dimensions", table: "Product");
            migrationBuilder.AddColumn<decimal>(
                name: "MinOrderValue",
                table: "Vouchers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
            migrationBuilder.AddColumn<int>(
                name: "TotalUsageLimit",
                table: "Vouchers",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "UsageLimitPerUser",
                table: "Vouchers",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "UsedCount",
                table: "Vouchers",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "ProductVariant",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                table: "ProductVariant",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "Width",
                table: "ProductVariant",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "Height", table: "Product", type: "decimal(18,2)", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "Length", table: "Product", type: "decimal(18,2)", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "Width", table: "Product", type: "decimal(18,2)", nullable: true);
            migrationBuilder.AddColumn<int>(name: "ProvinceId", table: "Output", type: "int", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ProvinceName",
                table: "Output",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "Output",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<string>(name: "WardCode", table: "Output", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "WardName", table: "Output", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "BookingAppointment",
                type: "nvarchar(30)",
                nullable: true);
            migrationBuilder.CreateTable(
                name: "OrderVoucher",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    VoucherId = table.Column<int>(type: "int", nullable: false),
                    OutputId = table.Column<int>(type: "int", nullable: false),
                    DiscountApplied = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AppliedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderVoucher", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderVoucher_Output_OutputId",
                        column: x => x.OutputId,
                        principalTable: "Output",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderVoucher_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "WarrantyTerm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    TermName = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    TermNameJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleType = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    ErrorCategory = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationMonths = table.Column<int>(type: "int", nullable: true),
                    DurationKm = table.Column<int>(type: "int", nullable: true),
                    Coverage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MediaUrl = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyTerm", x => x.Id);
                });
            migrationBuilder.CreateIndex(name: "IX_OrderVoucher_OutputId", table: "OrderVoucher", column: "OutputId");
            migrationBuilder.CreateIndex(name: "IX_OrderVoucher_VoucherId", table: "OrderVoucher", column: "VoucherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OrderVoucher");
            migrationBuilder.DropTable(name: "WarrantyTerm");
            migrationBuilder.DropColumn(name: "MinOrderValue", table: "Vouchers");
            migrationBuilder.DropColumn(name: "TotalUsageLimit", table: "Vouchers");
            migrationBuilder.DropColumn(name: "UsageLimitPerUser", table: "Vouchers");
            migrationBuilder.DropColumn(name: "UsedCount", table: "Vouchers");
            migrationBuilder.DropColumn(name: "Height", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "Length", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "Width", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "Height", table: "Product");
            migrationBuilder.DropColumn(name: "Length", table: "Product");
            migrationBuilder.DropColumn(name: "Width", table: "Product");
            migrationBuilder.DropColumn(name: "ProvinceId", table: "Output");
            migrationBuilder.DropColumn(name: "ProvinceName", table: "Output");
            migrationBuilder.DropColumn(name: "ShippingFee", table: "Output");
            migrationBuilder.DropColumn(name: "WardCode", table: "Output");
            migrationBuilder.DropColumn(name: "WardName", table: "Output");
            migrationBuilder.DropColumn(name: "ServiceType", table: "BookingAppointment");
            migrationBuilder.AddColumn<string>(
                name: "Dimensions",
                table: "ProductVariant",
                type: "nvarchar(35)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Dimensions",
                table: "Product",
                type: "nvarchar(35)",
                nullable: true);
        }
    }
}
