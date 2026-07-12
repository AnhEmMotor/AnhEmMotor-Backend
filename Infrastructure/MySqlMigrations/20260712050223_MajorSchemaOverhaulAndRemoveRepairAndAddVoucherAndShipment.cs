using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class MajorSchemaOverhaulAndRemoveRepairAndAddVoucherAndShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory");
            migrationBuilder.DropTable(name: "PlateDossier");
            migrationBuilder.DropTable(name: "RepairOrderDetail");
            migrationBuilder.DropTable(name: "RepairOrder");
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<long>(
                name: "PasswordResetTokenExpiry",
                table: "Users",
                type: "bigint",
                nullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "GroundClearance",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<string>(
                name: "FuelCapacity",
                table: "Product",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "DescriptionJson",
                table: "Product",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "MetaDescriptionJson",
                table: "Product",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "MetaTitleJson",
                table: "Product",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(name: "NameJson", table: "Product", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionJson",
                table: "Product",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "BudgetCode",
                table: "Output",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "CompanyAddress",
                table: "Output",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "CompanyEmail",
                table: "Output",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Output",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "CompanyTaxCode",
                table: "Output",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<bool>(
                name: "IsCompanyInvoice",
                table: "Output",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AddColumn<string>(
                name: "DescriptionJson",
                table: "Brand",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(name: "NameJson", table: "Brand", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "ProductCategoryTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategoryTranslations_ProductCategory_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TrackingNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Carrier = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerPhone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingCost = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),
                    DeliveredAt = table.Column<long>(type: "bigint", nullable: true),
                    OriginAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DestinationAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginLatitude = table.Column<double>(type: "double", nullable: true),
                    OriginLongitude = table.Column<double>(type: "double", nullable: true),
                    DestinationLatitude = table.Column<double>(type: "double", nullable: true),
                    DestinationLongitude = table.Column<double>(type: "double", nullable: true),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OutputId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_Output_OutputId",
                        column: x => x.OutputId,
                        principalTable: "Output",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApplyFor = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "ShipmentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShipmentId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentItems_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ShipmentItems_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ShipmentItems_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "VoucherLeads",
                columns: table => new
                {
                    VoucherId = table.Column<int>(type: "int", nullable: false),
                    LeadId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherLeads", x => new { x.VoucherId, x.LeadId });
                    table.ForeignKey(
                        name: "FK_VoucherLeads_Lead_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoucherLeads_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryTranslations_ProductCategoryId",
                table: "ProductCategoryTranslations",
                column: "ProductCategoryId");
            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ProductVariantColorId",
                table: "ShipmentItems",
                column: "ProductVariantColorId");
            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ProductVariantId",
                table: "ShipmentItems",
                column: "ProductVariantId");
            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ShipmentId",
                table: "ShipmentItems",
                column: "ShipmentId");
            migrationBuilder.CreateIndex(name: "IX_Shipments_OutputId", table: "Shipments", column: "OutputId");
            migrationBuilder.CreateIndex(name: "IX_VoucherLeads_LeadId", table: "VoucherLeads", column: "LeadId");
            migrationBuilder.CreateIndex(name: "IX_Vouchers_Code", table: "Vouchers", column: "Code", unique: true);
            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory",
                column: "TechnicianId",
                principalTable: "EmployeeProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory");
            migrationBuilder.DropTable(name: "ProductCategoryTranslations");
            migrationBuilder.DropTable(name: "ShipmentItems");
            migrationBuilder.DropTable(name: "VoucherLeads");
            migrationBuilder.DropTable(name: "Shipments");
            migrationBuilder.DropTable(name: "Vouchers");
            migrationBuilder.DropColumn(name: "PasswordResetToken", table: "Users");
            migrationBuilder.DropColumn(name: "PasswordResetTokenExpiry", table: "Users");
            migrationBuilder.DropColumn(name: "DescriptionJson", table: "Product");
            migrationBuilder.DropColumn(name: "MetaDescriptionJson", table: "Product");
            migrationBuilder.DropColumn(name: "MetaTitleJson", table: "Product");
            migrationBuilder.DropColumn(name: "NameJson", table: "Product");
            migrationBuilder.DropColumn(name: "ShortDescriptionJson", table: "Product");
            migrationBuilder.DropColumn(name: "BudgetCode", table: "Output");
            migrationBuilder.DropColumn(name: "CompanyAddress", table: "Output");
            migrationBuilder.DropColumn(name: "CompanyEmail", table: "Output");
            migrationBuilder.DropColumn(name: "CompanyName", table: "Output");
            migrationBuilder.DropColumn(name: "CompanyTaxCode", table: "Output");
            migrationBuilder.DropColumn(name: "IsCompanyInvoice", table: "Output");
            migrationBuilder.DropColumn(name: "DescriptionJson", table: "Brand");
            migrationBuilder.DropColumn(name: "NameJson", table: "Brand");
            migrationBuilder.AlterColumn<decimal>(
                name: "GroundClearance",
                table: "Product",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AlterColumn<decimal>(
                name: "FuelCapacity",
                table: "Product",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "PlateDossier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OutputId = table.Column<int>(type: "int", nullable: true),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompletedDate = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    CustomerName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerPhone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true),
                    DossierNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LicensePlate = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegistrationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ServiceFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    VinNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateDossier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlateDossier_Output_OutputId",
                        column: x => x.OutputId,
                        principalTable: "Output",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "RepairOrder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TechnicianId = table.Column<int>(type: "int", nullable: true),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    CompletedDate = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    CustomerName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerPhone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpectedCompletionTime = table.Column<long>(type: "bigint", nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Mileage = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PartsCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentStatus = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartTime = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairOrder_EmployeeProfile_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "EmployeeProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RepairOrder_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "RepairOrderDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    RepairOrderId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: true),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairOrderDetail_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RepairOrderDetail_RepairOrder_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairOrderDetail_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateIndex(name: "IX_PlateDossier_OutputId", table: "PlateDossier", column: "OutputId");
            migrationBuilder.CreateIndex(
                name: "IX_RepairOrder_TechnicianId",
                table: "RepairOrder",
                column: "TechnicianId");
            migrationBuilder.CreateIndex(name: "IX_RepairOrder_VehicleId", table: "RepairOrder", column: "VehicleId");
            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderDetail_ProductVariantId",
                table: "RepairOrderDetail",
                column: "ProductVariantId");
            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderDetail_RepairOrderId",
                table: "RepairOrderDetail",
                column: "RepairOrderId");
            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderDetail_ServiceId",
                table: "RepairOrderDetail",
                column: "ServiceId");
            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory",
                column: "TechnicianId",
                principalTable: "EmployeeProfile",
                principalColumn: "Id");
        }
    }
}
