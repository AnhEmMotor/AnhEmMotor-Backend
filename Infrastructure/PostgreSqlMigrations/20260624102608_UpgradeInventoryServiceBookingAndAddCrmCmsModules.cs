using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
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
            migrationBuilder.DropForeignKey(name: "FK_ServiceBooking_Services_ServiceId", table: "ServiceBooking");
            migrationBuilder.DropForeignKey(name: "FK_ServiceBooking_Users_CustomerId", table: "ServiceBooking");
            migrationBuilder.DropForeignKey(name: "FK_ServiceBooking_Vehicle_VehicleId", table: "ServiceBooking");
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceEvaluation_ServiceBooking_ServiceBookingId",
                table: "ServiceEvaluation");
            migrationBuilder.DropForeignKey(name: "FK_Vehicle_Product_ProductId", table: "Vehicle");
            migrationBuilder.DropTable(name: "SupplierDebtSettlements");
            migrationBuilder.DropIndex(name: "IX_InventoryReceiptInfo_SupplierId", table: "InventoryReceiptInfo");
            migrationBuilder.DropIndex(name: "IX_InventoryOnHand_ProductVariantId", table: "InventoryOnHand");
            migrationBuilder.DropPrimaryKey(name: "PK_ServiceBooking", table: "ServiceBooking");
            migrationBuilder.DropIndex(name: "IX_ServiceBooking_ServiceId", table: "ServiceBooking");
            migrationBuilder.DropIndex(name: "IX_ServiceBooking_TechnicianId", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "SupplierId", table: "InventoryReceiptInfo");
            migrationBuilder.DropColumn(name: "UnitPrice", table: "InventoryReceiptInfo");
            migrationBuilder.DropColumn(name: "CancelledDate", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "CancelledReason", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "CustomerNotes", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "DepositAmount", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "EstimatedDurationMinutes", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "Rating", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "Review", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "ServiceId", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "TechnicianId", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "TechnicianNotes", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "TotalAmount", table: "ServiceBooking");
            migrationBuilder.RenameTable(name: "ServiceBooking", newName: "ServiceBookings");
            migrationBuilder.RenameColumn(name: "ScheduledDate", table: "ServiceBookings", newName: "AppointmentDate");
            migrationBuilder.RenameColumn(name: "PaymentStatus", table: "ServiceBookings", newName: "ServiceType");
            migrationBuilder.RenameColumn(name: "CustomerId", table: "ServiceBookings", newName: "AssignedSaleId");
            migrationBuilder.RenameColumn(name: "CompletedDate", table: "ServiceBookings", newName: "CancelledAt");
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
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
            migrationBuilder.AddColumn<string>(
                name: "ElectronicWarrantyQrCode",
                table: "Vehicle",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<DateTime>(
                name: "LastMaintenanceDate",
                table: "Vehicle",
                type: "timestamp with time zone",
                nullable: true);
            migrationBuilder.AddColumn<DateTime>(
                name: "NextMaintenanceDate",
                table: "Vehicle",
                type: "timestamp with time zone",
                nullable: true);
            migrationBuilder.AddColumn<double>(
                name: "NextMaintenanceOdo",
                table: "Vehicle",
                type: "double precision",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "UserId", table: "Vehicle", type: "uuid", nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "ProductQuotationId",
                table: "PurchaseRequestItem",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "PurchaseRequestItem",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "PurchaseRequestItem",
                type: "numeric(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "MaxPurchaseQuantity",
                table: "ProductVariantColor",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "MaxPurchaseQuantity",
                table: "ProductVariant",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "MaxPurchaseQuantity",
                table: "Product",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Lead",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Lead",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<int>(
                name: "BeginningQty",
                table: "InventoryOnHand",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "InventoryOnHand",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "InventoryOnHand",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "ServiceBookings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ServiceBookings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ServiceBookings",
                type: "text",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "ServiceBookings",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<TimeSpan>(
                name: "AppointmentTime",
                table: "ServiceBookings",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "ServiceBookings",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<string>(
                name: "CustomerNote",
                table: "ServiceBookings",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddPrimaryKey(name: "PK_ServiceBookings", table: "ServiceBookings", column: "Id");
            migrationBuilder.CreateTable(
                name: "BookingAppointment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: true),
                    PreferredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreferredTimeSlot = table.Column<string>(type: "text", nullable: true),
                    AppointmentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Showroom = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConfirmedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "CustomerContact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcessedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RepliedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InternalNote = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "InventoryReceiptAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryReceiptId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ChangedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OldStatusId = table.Column<string>(type: "text", nullable: true),
                    NewStatusId = table.Column<string>(type: "text", nullable: true),
                    OldNotes = table.Column<string>(type: "text", nullable: true),
                    NewNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "InventoryReceiptInfoAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryReceiptInfoId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    OldQuantity = table.Column<int>(type: "integer", nullable: true),
                    NewQuantity = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptInfoAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfoAuditLog_InventoryReceiptInfo_Inventory~",
                        column: x => x.InventoryReceiptInfoId,
                        principalTable: "InventoryReceiptInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "InventoryTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    StockBefore = table.Column<int>(type: "integer", nullable: false),
                    StockAfter = table.Column<int>(type: "integer", nullable: false),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    ReferenceId = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    PerformedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "NewsArticle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Excerpt = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    SeoTitle = table.Column<string>(type: "text", nullable: true),
                    SeoDescription = table.Column<string>(type: "text", nullable: true),
                    SeoKeywords = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "OrderLogistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    CurrentStage = table.Column<int>(type: "integer", nullable: false),
                    BottleneckDescription = table.Column<string>(type: "text", nullable: false),
                    IsBottleneck = table.Column<bool>(type: "boolean", nullable: false),
                    DriverName = table.Column<string>(type: "text", nullable: false),
                    DriverPhone = table.Column<string>(type: "text", nullable: false),
                    CurrentLat = table.Column<double>(type: "double precision", nullable: false),
                    CurrentLng = table.Column<double>(type: "double precision", nullable: false),
                    EstimatedArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLogistics", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OutputId = table.Column<int>(type: "integer", nullable: false),
                    FromStatus = table.Column<string>(type: "text", nullable: true),
                    ToStatus = table.Column<string>(type: "text", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "PromotionBanner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    LinkUrl = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "PurchaseRequestAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseRequestId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ChangedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OldStatusId = table.Column<string>(type: "text", nullable: true),
                    NewStatusId = table.Column<string>(type: "text", nullable: true),
                    OldNotes = table.Column<string>(type: "text", nullable: true),
                    NewNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "PurchaseRequestItemAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseRequestItemId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    OldQuantity = table.Column<int>(type: "integer", nullable: true),
                    NewQuantity = table.Column<int>(type: "integer", nullable: true),
                    OldProductVariantId = table.Column<int>(type: "integer", nullable: true),
                    NewProductVariantId = table.Column<int>(type: "integer", nullable: true),
                    OldProductVariantColorId = table.Column<int>(type: "integer", nullable: true),
                    NewProductVariantColorId = table.Column<int>(type: "integer", nullable: true),
                    OldSupplierName = table.Column<string>(type: "text", nullable: true),
                    NewSupplierName = table.Column<string>(type: "text", nullable: true),
                    OldUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    NewUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequestItemAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItemAuditLog_PurchaseRequestItem_PurchaseReq~",
                        column: x => x.PurchaseRequestItemId,
                        principalTable: "PurchaseRequestItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "SupplierDebtLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingDebt = table.Column<decimal>(
                        type: "numeric(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),
                    PaymentDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "SupportTicket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SLADeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "VehicleAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ChangedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OldVinNumber = table.Column<string>(type: "text", nullable: true),
                    NewVinNumber = table.Column<string>(type: "text", nullable: true),
                    OldEngineNumber = table.Column<string>(type: "text", nullable: true),
                    NewEngineNumber = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "CustomerContactReply",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContactId = table.Column<int>(type: "integer", nullable: false),
                    ReplyContent = table.Column<string>(type: "text", nullable: false),
                    IsInternal = table.Column<bool>(type: "boolean", nullable: false),
                    RepliedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SupportTicketId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateIndex(name: "IX_Vehicle_UserId", table: "Vehicle", column: "UserId");
            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_ProductQuotationId",
                table: "PurchaseRequestItem",
                column: "ProductQuotationId");
            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_SupplierId",
                table: "PurchaseRequestItem",
                column: "SupplierId");
            migrationBuilder.CreateIndex(
                name: "IX_InventoryOnHand_ProductVariantId_ProductVariantColorId_Mont~",
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
            migrationBuilder.CreateIndex(name: "IX_NewsArticle_AuthorId", table: "NewsArticle", column: "AuthorId");
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
            migrationBuilder.DropForeignKey(name: "FK_ServiceBookings_Users_AssignedSaleId", table: "ServiceBookings");
            migrationBuilder.DropForeignKey(name: "FK_ServiceBookings_Vehicle_VehicleId", table: "ServiceBookings");
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceEvaluation_ServiceBookings_ServiceBookingId",
                table: "ServiceEvaluation");
            migrationBuilder.DropForeignKey(name: "FK_Vehicle_Product_ProductId", table: "Vehicle");
            migrationBuilder.DropForeignKey(name: "FK_Vehicle_Users_UserId", table: "Vehicle");
            migrationBuilder.DropTable(name: "BookingAppointment");
            migrationBuilder.DropTable(name: "CustomerContactReply");
            migrationBuilder.DropTable(name: "InventoryReceiptAuditLog");
            migrationBuilder.DropTable(name: "InventoryReceiptInfoAuditLog");
            migrationBuilder.DropTable(name: "InventoryTransaction");
            migrationBuilder.DropTable(name: "NewsArticle");
            migrationBuilder.DropTable(name: "OrderLogistics");
            migrationBuilder.DropTable(name: "OrderStatusHistory");
            migrationBuilder.DropTable(name: "PromotionBanner");
            migrationBuilder.DropTable(name: "PurchaseRequestAuditLog");
            migrationBuilder.DropTable(name: "PurchaseRequestItemAuditLog");
            migrationBuilder.DropTable(name: "SupplierDebtLog");
            migrationBuilder.DropTable(name: "VehicleAuditLog");
            migrationBuilder.DropTable(name: "CustomerContact");
            migrationBuilder.DropTable(name: "SupportTicket");
            migrationBuilder.DropIndex(name: "IX_Vehicle_UserId", table: "Vehicle");
            migrationBuilder.DropIndex(name: "IX_PurchaseRequestItem_ProductQuotationId", table: "PurchaseRequestItem");
            migrationBuilder.DropIndex(name: "IX_PurchaseRequestItem_SupplierId", table: "PurchaseRequestItem");
            migrationBuilder.DropIndex(
                name: "IX_InventoryOnHand_ProductVariantId_ProductVariantColorId_Mont~",
                table: "InventoryOnHand");
            migrationBuilder.DropPrimaryKey(name: "PK_ServiceBookings", table: "ServiceBookings");
            migrationBuilder.DropColumn(name: "CurrentOdo", table: "Vehicle");
            migrationBuilder.DropColumn(name: "ElectronicWarrantyQrCode", table: "Vehicle");
            migrationBuilder.DropColumn(name: "LastMaintenanceDate", table: "Vehicle");
            migrationBuilder.DropColumn(name: "NextMaintenanceDate", table: "Vehicle");
            migrationBuilder.DropColumn(name: "NextMaintenanceOdo", table: "Vehicle");
            migrationBuilder.DropColumn(name: "UserId", table: "Vehicle");
            migrationBuilder.DropColumn(name: "ProductQuotationId", table: "PurchaseRequestItem");
            migrationBuilder.DropColumn(name: "SupplierId", table: "PurchaseRequestItem");
            migrationBuilder.DropColumn(name: "UnitPrice", table: "PurchaseRequestItem");
            migrationBuilder.DropColumn(name: "MaxPurchaseQuantity", table: "ProductVariantColor");
            migrationBuilder.DropColumn(name: "MaxPurchaseQuantity", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "MaxPurchaseQuantity", table: "Product");
            migrationBuilder.DropColumn(name: "Notes", table: "Lead");
            migrationBuilder.DropColumn(name: "Priority", table: "Lead");
            migrationBuilder.DropColumn(name: "BeginningQty", table: "InventoryOnHand");
            migrationBuilder.DropColumn(name: "Month", table: "InventoryOnHand");
            migrationBuilder.DropColumn(name: "Year", table: "InventoryOnHand");
            migrationBuilder.DropColumn(name: "AdminNote", table: "ServiceBookings");
            migrationBuilder.DropColumn(name: "AppointmentTime", table: "ServiceBookings");
            migrationBuilder.DropColumn(name: "CancellationReason", table: "ServiceBookings");
            migrationBuilder.DropColumn(name: "CustomerNote", table: "ServiceBookings");
            migrationBuilder.RenameTable(name: "ServiceBookings", newName: "ServiceBooking");
            migrationBuilder.RenameColumn(name: "ServiceType", table: "ServiceBooking", newName: "PaymentStatus");
            migrationBuilder.RenameColumn(name: "CancelledAt", table: "ServiceBooking", newName: "CompletedDate");
            migrationBuilder.RenameColumn(name: "AssignedSaleId", table: "ServiceBooking", newName: "CustomerId");
            migrationBuilder.RenameColumn(name: "AppointmentDate", table: "ServiceBooking", newName: "ScheduledDate");
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
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "InventoryReceiptInfo",
                type: "numeric(18,2)",
                nullable: true);
            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "ServiceBooking",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ServiceBooking",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ServiceBooking",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledDate",
                table: "ServiceBooking",
                type: "timestamp with time zone",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "CancelledReason",
                table: "ServiceBooking",
                type: "text",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "CustomerNotes",
                table: "ServiceBooking",
                type: "text",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "ServiceBooking",
                type: "numeric(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "ServiceBooking",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<int>(name: "Rating", table: "ServiceBooking", type: "integer", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Review", table: "ServiceBooking", type: "text", nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "ServiceBooking",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "ServiceBooking",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "TechnicianNotes",
                table: "ServiceBooking",
                type: "text",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "ServiceBooking",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
            migrationBuilder.AddPrimaryKey(name: "PK_ServiceBooking", table: "ServiceBooking", column: "Id");
            migrationBuilder.CreateTable(
                name: "SupplierDebtSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    PaymentDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfo_SupplierId",
                table: "InventoryReceiptInfo",
                column: "SupplierId");
            migrationBuilder.CreateIndex(
                name: "IX_InventoryOnHand_ProductVariantId",
                table: "InventoryOnHand",
                column: "ProductVariantId");
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
