using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class UpgradeInventoryServiceBookingAndAddCrmCmsModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptInfo_Supplier_SupplierId",
                table: "InventoryReceiptInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceBooking_EmployeeProfile_TechnicianId",
                table: "ServiceBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceBooking_Services_ServiceId",
                table: "ServiceBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceBooking_Users_CustomerId",
                table: "ServiceBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceBooking_Vehicle_VehicleId",
                table: "ServiceBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceEvaluation_ServiceBooking_ServiceBookingId",
                table: "ServiceEvaluation");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Product_ProductId",
                table: "Vehicle");

            migrationBuilder.DropTable(
                name: "SupplierDebtSettlements");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceiptInfo_SupplierId",
                table: "InventoryReceiptInfo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceBooking",
                table: "ServiceBooking");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBooking_ServiceId",
                table: "ServiceBooking");

            migrationBuilder.DropIndex(
                name: "IX_ServiceBooking_TechnicianId",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "InventoryReceiptInfo");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "InventoryReceiptInfo");

            migrationBuilder.DropColumn(
                name: "CancelledDate",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "CancelledReason",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "CustomerNotes",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "Review",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "TechnicianNotes",
                table: "ServiceBooking");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "ServiceBooking");

            migrationBuilder.RenameTable(
                name: "ServiceBooking",
                newName: "ServiceBookings");

            migrationBuilder.RenameColumn(
                name: "PaymentStatus",
                table: "ServiceBookings",
                newName: "ServiceType");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "ServiceBookings",
                newName: "AssignedSaleId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceBooking_VehicleId",
                table: "ServiceBookings",
                newName: "IX_ServiceBookings_VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceBooking_CustomerId",
                table: "ServiceBookings",
                newName: "IX_ServiceBookings_AssignedSaleId");

            migrationBuilder.AddColumn<double>(
                name: "CurrentOdo",
                table: "Vehicle",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ElectronicWarrantyQrCode",
                table: "Vehicle",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMaintenanceDate",
                table: "Vehicle",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextMaintenanceDate",
                table: "Vehicle",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NextMaintenanceOdo",
                table: "Vehicle",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Vehicle",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "ProductQuotationId",
                table: "PurchaseRequestItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "PurchaseRequestItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "PurchaseRequestItem",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPurchaseQuantity",
                table: "ProductVariantColor",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPurchaseQuantity",
                table: "ProductVariant",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPurchaseQuantity",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Lead",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Lead",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "BeginningQty",
                table: "InventoryOnHand",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "InventoryOnHand",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "InventoryOnHand",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "ServiceBookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "ServiceBookings",
                keyColumn: "Notes",
                keyValue: null,
                column: "Notes",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ServiceBookings",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "ServiceBookings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDate",
                table: "ServiceBookings",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AppointmentTime",
                table: "ServiceBookings",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "ServiceBookings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "ServiceBookings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNote",
                table: "ServiceBookings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceBookings",
                table: "ServiceBookings",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BookingAppointment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FullName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    PreferredDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PreferredTimeSlot = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppointmentAt = table.Column<long>(type: "bigint", nullable: true),
                    Showroom = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConfirmedAt = table.Column<long>(type: "bigint", nullable: true),
                    ConfirmedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CancelReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingAppointment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingAppointment_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BookingAppointment_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomerContact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FullName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Source = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessedAt = table.Column<long>(type: "bigint", nullable: true),
                    ProcessedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RepliedAt = table.Column<long>(type: "bigint", nullable: true),
                    InternalNote = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerContact_Users_ProcessedBy",
                        column: x => x.ProcessedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventoryReceiptAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InventoryReceiptId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ChangedAt = table.Column<long>(type: "bigint", nullable: false),
                    OldStatusId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewStatusId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptAuditLog_InventoryReceipt_InventoryReceiptId",
                        column: x => x.InventoryReceiptId,
                        principalTable: "InventoryReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptAuditLog_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventoryReceiptInfoAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InventoryReceiptInfoId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldQuantity = table.Column<int>(type: "int", nullable: true),
                    NewQuantity = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptInfoAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfoAuditLog_InventoryReceiptInfo_InventoryR~",
                        column: x => x.InventoryReceiptInfoId,
                        principalTable: "InventoryReceiptInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventoryTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StockBefore = table.Column<int>(type: "int", nullable: false),
                    StockAfter = table.Column<int>(type: "int", nullable: false),
                    ReferenceType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PerformedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PerformedAt = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransaction_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransaction_Users_PerformedBy",
                        column: x => x.PerformedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NewsArticle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slug = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Excerpt = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoverImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SeoTitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SeoDescription = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SeoKeywords = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsFeatured = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<long>(type: "bigint", nullable: true),
                    PublishedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AuthorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsArticle_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NewsArticle_Users_PublishedBy",
                        column: x => x.PublishedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderLogistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    BottleneckDescription = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsBottleneck = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DriverName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DriverPhone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrentLat = table.Column<double>(type: "double", nullable: false),
                    CurrentLng = table.Column<double>(type: "double", nullable: false),
                    EstimatedArrivalTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLogistics", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OutputId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToStatus = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ChangedAt = table.Column<long>(type: "bigint", nullable: true),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Output_OutputId",
                        column: x => x.OutputId,
                        principalTable: "Output",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PromotionBanner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LinkUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<long>(type: "bigint", nullable: false),
                    EndDate = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionBanner", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionBanner_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PromotionBanner_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PurchaseRequestAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PurchaseRequestId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ChangedAt = table.Column<long>(type: "bigint", nullable: false),
                    OldStatusId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewStatusId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequestAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestAuditLog_PurchaseRequest_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestAuditLog_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PurchaseRequestItemAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PurchaseRequestItemId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldQuantity = table.Column<int>(type: "int", nullable: true),
                    NewQuantity = table.Column<int>(type: "int", nullable: true),
                    OldProductVariantId = table.Column<int>(type: "int", nullable: true),
                    NewProductVariantId = table.Column<int>(type: "int", nullable: true),
                    OldProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    NewProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    OldSupplierName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewSupplierName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NewUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequestItemAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItemAuditLog_PurchaseRequestItem_PurchaseRequ~",
                        column: x => x.PurchaseRequestItemId,
                        principalTable: "PurchaseRequestItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SupplierDebtLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingDebt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<long>(type: "bigint", nullable: false),
                    CreatedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDebtLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDebtLog_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierDebtLog_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SupportTicket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Subject = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResolvedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SLADeadline = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AssignedAdminId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTicket_Users_AssignedAdminId",
                        column: x => x.AssignedAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupportTicket_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VehicleAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ChangedAt = table.Column<long>(type: "bigint", nullable: false),
                    OldVinNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewVinNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldEngineNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewEngineNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleAuditLog_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehicleAuditLog_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomerContactReply",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    ReplyContent = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsInternal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RepliedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    SentAt = table.Column<long>(type: "bigint", nullable: true),
                    SupportTicketId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerContactReply", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerContactReply_CustomerContact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "CustomerContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerContactReply_SupportTicket_SupportTicketId",
                        column: x => x.SupportTicketId,
                        principalTable: "SupportTicket",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomerContactReply_Users_RepliedBy",
                        column: x => x.RepliedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_UserId",
                table: "Vehicle",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_ProductQuotationId",
                table: "PurchaseRequestItem",
                column: "ProductQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_SupplierId",
                table: "PurchaseRequestItem",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOnHand_ProductVariantId_ProductVariantColorId_Month~",
                table: "InventoryOnHand",
                columns: new[] { "ProductVariantId", "ProductVariantColorId", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingAppointment_ConfirmedBy",
                table: "BookingAppointment",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAppointment_ProductVariantId",
                table: "BookingAppointment",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAppointment_Status_AppointmentAt",
                table: "BookingAppointment",
                columns: new[] { "Status", "AppointmentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContact_ProcessedBy",
                table: "CustomerContact",
                column: "ProcessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContact_Status_CreatedAt",
                table: "CustomerContact",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContactReply_ContactId_SentAt",
                table: "CustomerContactReply",
                columns: new[] { "ContactId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContactReply_RepliedBy",
                table: "CustomerContactReply",
                column: "RepliedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContactReply_SupportTicketId",
                table: "CustomerContactReply",
                column: "SupportTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptAuditLog_ChangedById",
                table: "InventoryReceiptAuditLog",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptAuditLog_InventoryReceiptId",
                table: "InventoryReceiptAuditLog",
                column: "InventoryReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfoAuditLog_InventoryReceiptInfoId",
                table: "InventoryReceiptInfoAuditLog",
                column: "InventoryReceiptInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_PerformedBy",
                table: "InventoryTransaction",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_ProductVariantId_PerformedAt",
                table: "InventoryTransaction",
                columns: new[] { "ProductVariantId", "PerformedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_AuthorId",
                table: "NewsArticle",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_PublishedBy",
                table: "NewsArticle",
                column: "PublishedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_Slug",
                table: "NewsArticle",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_Status_PublishedAt",
                table: "NewsArticle",
                columns: new[] { "Status", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_ChangedBy",
                table: "OrderStatusHistory",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_OutputId_ChangedAt",
                table: "OrderStatusHistory",
                columns: new[] { "OutputId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBanner_CreatedBy",
                table: "PromotionBanner",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBanner_IsEnabled_StartDate_EndDate",
                table: "PromotionBanner",
                columns: new[] { "IsEnabled", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBanner_UpdatedBy",
                table: "PromotionBanner",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestAuditLog_ChangedById",
                table: "PurchaseRequestAuditLog",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestAuditLog_PurchaseRequestId",
                table: "PurchaseRequestAuditLog",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItemAuditLog_PurchaseRequestItemId",
                table: "PurchaseRequestItemAuditLog",
                column: "PurchaseRequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebtLog_CreatedById",
                table: "SupplierDebtLog",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebtLog_SupplierId",
                table: "SupplierDebtLog",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_AssignedAdminId",
                table: "SupportTicket",
                column: "AssignedAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_CustomerId",
                table: "SupportTicket",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAuditLog_ChangedById",
                table: "VehicleAuditLog",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAuditLog_VehicleId",
                table: "VehicleAuditLog",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequestItem_ProductQuotations_ProductQuotationId",
                table: "PurchaseRequestItem",
                column: "ProductQuotationId",
                principalTable: "ProductQuotations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequestItem_Supplier_SupplierId",
                table: "PurchaseRequestItem",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBookings_Users_AssignedSaleId",
                table: "ServiceBookings",
                column: "AssignedSaleId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBookings_Vehicle_VehicleId",
                table: "ServiceBookings",
                column: "VehicleId",
                principalTable: "Vehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceEvaluation_ServiceBookings_ServiceBookingId",
                table: "ServiceEvaluation",
                column: "ServiceBookingId",
                principalTable: "ServiceBookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Product_ProductId",
                table: "Vehicle",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Users_UserId",
                table: "Vehicle",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequestItem_ProductQuotations_ProductQuotationId",
                table: "PurchaseRequestItem");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequestItem_Supplier_SupplierId",
                table: "PurchaseRequestItem");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceBookings_Users_AssignedSaleId",
                table: "ServiceBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceBookings_Vehicle_VehicleId",
                table: "ServiceBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceEvaluation_ServiceBookings_ServiceBookingId",
                table: "ServiceEvaluation");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Product_ProductId",
                table: "Vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Users_UserId",
                table: "Vehicle");

            migrationBuilder.DropTable(
                name: "BookingAppointment");

            migrationBuilder.DropTable(
                name: "CustomerContactReply");

            migrationBuilder.DropTable(
                name: "InventoryReceiptAuditLog");

            migrationBuilder.DropTable(
                name: "InventoryReceiptInfoAuditLog");

            migrationBuilder.DropTable(
                name: "InventoryTransaction");

            migrationBuilder.DropTable(
                name: "NewsArticle");

            migrationBuilder.DropTable(
                name: "OrderLogistics");

            migrationBuilder.DropTable(
                name: "OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "PromotionBanner");

            migrationBuilder.DropTable(
                name: "PurchaseRequestAuditLog");

            migrationBuilder.DropTable(
                name: "PurchaseRequestItemAuditLog");

            migrationBuilder.DropTable(
                name: "SupplierDebtLog");

            migrationBuilder.DropTable(
                name: "VehicleAuditLog");

            migrationBuilder.DropTable(
                name: "CustomerContact");

            migrationBuilder.DropTable(
                name: "SupportTicket");

            migrationBuilder.DropIndex(
                name: "IX_Vehicle_UserId",
                table: "Vehicle");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequestItem_ProductQuotationId",
                table: "PurchaseRequestItem");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequestItem_SupplierId",
                table: "PurchaseRequestItem");

            migrationBuilder.DropIndex(
                name: "IX_InventoryOnHand_ProductVariantId_ProductVariantColorId_Month~",
                table: "InventoryOnHand");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceBookings",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CurrentOdo",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "ElectronicWarrantyQrCode",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "LastMaintenanceDate",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceDate",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceOdo",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "ProductQuotationId",
                table: "PurchaseRequestItem");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "PurchaseRequestItem");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "PurchaseRequestItem");

            migrationBuilder.DropColumn(
                name: "MaxPurchaseQuantity",
                table: "ProductVariantColor");

            migrationBuilder.DropColumn(
                name: "MaxPurchaseQuantity",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "MaxPurchaseQuantity",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Lead");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Lead");

            migrationBuilder.DropColumn(
                name: "BeginningQty",
                table: "InventoryOnHand");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "InventoryOnHand");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "InventoryOnHand");

            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "AppointmentTime",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "ServiceBookings");

            migrationBuilder.DropColumn(
                name: "CustomerNote",
                table: "ServiceBookings");

            migrationBuilder.RenameTable(
                name: "ServiceBookings",
                newName: "ServiceBooking");

            migrationBuilder.RenameColumn(
                name: "ServiceType",
                table: "ServiceBooking",
                newName: "PaymentStatus");

            migrationBuilder.RenameColumn(
                name: "AssignedSaleId",
                table: "ServiceBooking",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceBookings_VehicleId",
                table: "ServiceBooking",
                newName: "IX_ServiceBooking_VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceBookings_AssignedSaleId",
                table: "ServiceBooking",
                newName: "IX_ServiceBooking_CustomerId");

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "InventoryReceiptInfo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "InventoryReceiptInfo",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "ServiceBooking",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ServiceBooking",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "CancelledDate",
                table: "ServiceBooking",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledReason",
                table: "ServiceBooking",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "CompletedDate",
                table: "ServiceBooking",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNotes",
                table: "ServiceBooking",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "ServiceBooking",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "ServiceBooking",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "ServiceBooking",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Review",
                table: "ServiceBooking",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "ScheduledDate",
                table: "ServiceBooking",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "ServiceBooking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "ServiceBooking",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicianNotes",
                table: "ServiceBooking",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "ServiceBooking",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceBooking",
                table: "ServiceBooking",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SupplierDebtSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true),
                    EvidenceUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentDate = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfo_SupplierId",
                table: "InventoryReceiptInfo",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBooking_ServiceId",
                table: "ServiceBooking",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBooking_TechnicianId",
                table: "ServiceBooking",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebtSettlements_SupplierId",
                table: "SupplierDebtSettlements",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceiptInfo_Supplier_SupplierId",
                table: "InventoryReceiptInfo",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBooking_EmployeeProfile_TechnicianId",
                table: "ServiceBooking",
                column: "TechnicianId",
                principalTable: "EmployeeProfile",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBooking_Services_ServiceId",
                table: "ServiceBooking",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBooking_Users_CustomerId",
                table: "ServiceBooking",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBooking_Vehicle_VehicleId",
                table: "ServiceBooking",
                column: "VehicleId",
                principalTable: "Vehicle",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceEvaluation_ServiceBooking_ServiceBookingId",
                table: "ServiceEvaluation",
                column: "ServiceBookingId",
                principalTable: "ServiceBooking",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Product_ProductId",
                table: "Vehicle",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");
        }
    }
}
