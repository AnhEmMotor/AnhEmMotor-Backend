using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.MySqlMigrations
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
            migrationBuilder.AddColumn<string>(name: "ProvinceName", table: "Output", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "Output",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<string>(name: "WardCode", table: "Output", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(name: "WardName", table: "Output", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "BookingAppointment",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "OrderVoucher",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VoucherId = table.Column<int>(type: "int", nullable: false),
                    OutputId = table.Column<int>(type: "int", nullable: false),
                    DiscountApplied = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AppliedAt = table.Column<long>(type: "bigint", nullable: false),
                    AppliedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "WarrantyTerm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    TermName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TermNameJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VehicleType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorCategory = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescriptionJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DurationMonths = table.Column<int>(type: "int", nullable: true),
                    DurationKm = table.Column<int>(type: "int", nullable: true),
                    Coverage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EffectiveDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MediaUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyTerm", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
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
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(name: "Dimensions", table: "Product", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
