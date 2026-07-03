using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
{
    /// <inheritdoc />
    public partial class AddSalesAndWorkshopInvoicesAndWarranty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_PlateDossier_Output_OutputId", table: "PlateDossier");
            migrationBuilder.DropTable(name: "ContractTemplateAuditLog");
            migrationBuilder.DropTable(name: "ServiceEvaluation");
            migrationBuilder.DropTable(name: "ContractTemplates");
            migrationBuilder.AlterColumn<int>(
                name: "OutputId",
                table: "PlateDossier",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedDate",
                table: "PlateDossier",
                type: "timestamp with time zone",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "PlateDossier",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "PlateDossier",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<string>(
                name: "DossierNumber",
                table: "PlateDossier",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<string>(
                name: "VinNumber",
                table: "PlateDossier",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "ParcelDeliveryOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ParcelDeliveryOrders",
                type: "text",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "ReturnShippingCost",
                table: "ParcelDeliveryOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
            migrationBuilder.AddColumn<int>(name: "LeadId", table: "Output", type: "integer", nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "LaborCost",
                table: "MaintenanceHistory",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
            migrationBuilder.AddColumn<string>(
                name: "MaintenanceNumber",
                table: "MaintenanceHistory",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextMaintenanceDate",
                table: "MaintenanceHistory",
                type: "timestamp with time zone",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "NextMaintenanceOdo",
                table: "MaintenanceHistory",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "PartsCost",
                table: "MaintenanceHistory",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
            migrationBuilder.AddColumn<string>(
                name: "PartsJson",
                table: "MaintenanceHistory",
                type: "text",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "MaintenanceHistory",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "MaintenanceHistory",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Lead",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AddColumn<string>(
                name: "PricingRulesJson",
                table: "CarrierPartners",
                type: "text",
                nullable: true);
            migrationBuilder.AddColumn<string>(name: "SlaJson", table: "CarrierPartners", type: "text", nullable: true);
            migrationBuilder.CreateTable(
                name: "ConversionTool",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    DelaySeconds = table.Column<int>(type: "integer", nullable: true),
                    Pages = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Views = table.Column<int>(type: "integer", nullable: false),
                    Clicks = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Leads = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversionTool", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerName = table.Column<string>(type: "text", nullable: false),
                    CustomerIdCard = table.Column<string>(type: "text", nullable: false),
                    CustomerPhone = table.Column<string>(type: "text", nullable: false),
                    CustomerAddress = table.Column<string>(type: "text", nullable: false),
                    VehicleModel = table.Column<string>(type: "text", nullable: false),
                    VehicleColor = table.Column<string>(type: "text", nullable: false),
                    ChassisNo = table.Column<string>(type: "text", nullable: false),
                    EngineNo = table.Column<string>(type: "text", nullable: false),
                    VehiclePrice = table.Column<decimal>(
                        type: "numeric(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),
                    RegistrationFee = table.Column<decimal>(
                        type: "numeric(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),
                    InsuranceFee = table.Column<decimal>(
                        type: "numeric(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ProcessedBy = table.Column<string>(type: "text", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SalesPerson = table.Column<string>(type: "text", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoice_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "PurchaseInvoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: false),
                    PurchaseRequestId = table.Column<int>(type: "integer", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    SupplierName = table.Column<string>(type: "text", nullable: true),
                    SupplierPhone = table.Column<string>(type: "text", nullable: true),
                    SupplierAddress = table.Column<string>(type: "text", nullable: true),
                    SupplierTaxCode = table.Column<string>(type: "text", nullable: true),
                    CustomerName = table.Column<string>(type: "text", nullable: true),
                    CustomerPhone = table.Column<string>(type: "text", nullable: true),
                    CustomerAddress = table.Column<string>(type: "text", nullable: true),
                    CustomerIdCard = table.Column<string>(type: "text", nullable: true),
                    InvoiceDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoice_PurchaseRequest_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequest",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseInvoice_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id");
                });
            migrationBuilder.CreateTable(
                name: "ReturnRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    OrderCode = table.Column<string>(type: "text", nullable: false),
                    OriginalTrackingNumber = table.Column<string>(type: "text", nullable: false),
                    CustomerName = table.Column<string>(type: "text", nullable: false),
                    CustomerPhone = table.Column<string>(type: "text", nullable: false),
                    Carrier = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CancelReason = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ReturnAction = table.Column<string>(type: "text", nullable: true),
                    EvidenceImagesJson = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    InspectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnRequest_Output_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Output",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "SupplierDebtLogImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierDebtLogId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDebtLogImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDebtLogImages_SupplierDebtLog_SupplierDebtLogId",
                        column: x => x.SupplierDebtLogId,
                        principalTable: "SupplierDebtLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "WarrantyClaim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClaimNumber = table.Column<string>(type: "text", nullable: false),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    IssueDescription = table.Column<string>(type: "text", nullable: false),
                    MediaUrls = table.Column<string>(type: "text", nullable: true),
                    ServiceCenterName = table.Column<string>(type: "text", nullable: true),
                    ManufacturerClaimNumber = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ManufacturerDecision = table.Column<string>(type: "text", nullable: true),
                    IsRecall = table.Column<bool>(type: "boolean", nullable: false),
                    TotalPartsCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalLaborCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyClaim_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "WorkshopPayment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentNumber = table.Column<string>(type: "text", nullable: false),
                    SourceType = table.Column<string>(type: "text", nullable: false),
                    SourceId = table.Column<int>(type: "integer", nullable: false),
                    CustomerName = table.Column<string>(type: "text", nullable: false),
                    CustomerPhone = table.Column<string>(type: "text", nullable: false),
                    VehicleInfo = table.Column<string>(type: "text", nullable: true),
                    ServiceDescription = table.Column<string>(type: "text", nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    PaymentStatus = table.Column<string>(type: "text", nullable: false),
                    ReceivedById = table.Column<Guid>(type: "uuid", nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    InvoicePrintedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopPayment", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    PurchaseRequestItemId = table.Column<int>(type: "integer", nullable: true),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "integer", nullable: true),
                    ProductName = table.Column<string>(type: "text", nullable: true),
                    VariantName = table.Column<string>(type: "text", nullable: true),
                    ColorName = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceItem_PurchaseInvoice_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "ReturnRequestItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReturnRequestId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    Sku = table.Column<string>(type: "text", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ReturnQuantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequestItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnRequestItem_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnRequestItem_ReturnRequest_ReturnRequestId",
                        column: x => x.ReturnRequestId,
                        principalTable: "ReturnRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "WarrantyClaimPart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WarrantyClaimId = table.Column<int>(type: "integer", nullable: false),
                    PartName = table.Column<string>(type: "text", nullable: false),
                    PartCode = table.Column<string>(type: "text", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyClaimPart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyClaimPart_WarrantyClaim_WarrantyClaimId",
                        column: x => x.WarrantyClaimId,
                        principalTable: "WarrantyClaim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(
                name: "IX_SupportRequest_AssignedUserId",
                table: "SupportRequest",
                column: "AssignedUserId");
            migrationBuilder.CreateIndex(name: "IX_Output_LeadId", table: "Output", column: "LeadId");
            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceHistory_TechnicianId",
                table: "MaintenanceHistory",
                column: "TechnicianId");
            migrationBuilder.CreateIndex(name: "IX_Invoice_UserId", table: "Invoice", column: "UserId");
            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_PurchaseRequestId",
                table: "PurchaseInvoice",
                column: "PurchaseRequestId");
            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_SupplierId",
                table: "PurchaseInvoice",
                column: "SupplierId");
            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItem_PurchaseInvoiceId",
                table: "PurchaseInvoiceItem",
                column: "PurchaseInvoiceId");
            migrationBuilder.CreateIndex(name: "IX_ReturnRequest_OrderId", table: "ReturnRequest", column: "OrderId");
            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequestItem_ProductId",
                table: "ReturnRequestItem",
                column: "ProductId");
            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequestItem_ReturnRequestId",
                table: "ReturnRequestItem",
                column: "ReturnRequestId");
            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebtLogImages_SupplierDebtLogId",
                table: "SupplierDebtLogImages",
                column: "SupplierDebtLogId");
            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaim_VehicleId",
                table: "WarrantyClaim",
                column: "VehicleId");
            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaimPart_WarrantyClaimId",
                table: "WarrantyClaimPart",
                column: "WarrantyClaimId");
            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory",
                column: "TechnicianId",
                principalTable: "EmployeeProfile",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_Output_Lead_LeadId",
                table: "Output",
                column: "LeadId",
                principalTable: "Lead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_PlateDossier_Output_OutputId",
                table: "PlateDossier",
                column: "OutputId",
                principalTable: "Output",
                principalColumn: "id");
            migrationBuilder.AddForeignKey(
                name: "FK_SupportRequest_Users_AssignedUserId",
                table: "SupportRequest",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory");
            migrationBuilder.DropForeignKey(name: "FK_Output_Lead_LeadId", table: "Output");
            migrationBuilder.DropForeignKey(name: "FK_PlateDossier_Output_OutputId", table: "PlateDossier");
            migrationBuilder.DropForeignKey(name: "FK_SupportRequest_Users_AssignedUserId", table: "SupportRequest");
            migrationBuilder.DropTable(name: "ConversionTool");
            migrationBuilder.DropTable(name: "Invoice");
            migrationBuilder.DropTable(name: "PurchaseInvoiceItem");
            migrationBuilder.DropTable(name: "ReturnRequestItem");
            migrationBuilder.DropTable(name: "SupplierDebtLogImages");
            migrationBuilder.DropTable(name: "WarrantyClaimPart");
            migrationBuilder.DropTable(name: "WorkshopPayment");
            migrationBuilder.DropTable(name: "PurchaseInvoice");
            migrationBuilder.DropTable(name: "ReturnRequest");
            migrationBuilder.DropTable(name: "WarrantyClaim");
            migrationBuilder.DropIndex(name: "IX_SupportRequest_AssignedUserId", table: "SupportRequest");
            migrationBuilder.DropIndex(name: "IX_Output_LeadId", table: "Output");
            migrationBuilder.DropIndex(name: "IX_MaintenanceHistory_TechnicianId", table: "MaintenanceHistory");
            migrationBuilder.DropColumn(name: "CompletedDate", table: "PlateDossier");
            migrationBuilder.DropColumn(name: "CustomerName", table: "PlateDossier");
            migrationBuilder.DropColumn(name: "CustomerPhone", table: "PlateDossier");
            migrationBuilder.DropColumn(name: "DossierNumber", table: "PlateDossier");
            migrationBuilder.DropColumn(name: "VinNumber", table: "PlateDossier");
            migrationBuilder.DropColumn(name: "RefundAmount", table: "ParcelDeliveryOrders");
            migrationBuilder.DropColumn(name: "RejectionReason", table: "ParcelDeliveryOrders");
            migrationBuilder.DropColumn(name: "ReturnShippingCost", table: "ParcelDeliveryOrders");
            migrationBuilder.DropColumn(name: "LeadId", table: "Output");
            migrationBuilder.DropColumn(name: "LaborCost", table: "MaintenanceHistory");
            migrationBuilder.DropColumn(name: "MaintenanceNumber", table: "MaintenanceHistory");
            migrationBuilder.DropColumn(name: "NextMaintenanceDate", table: "MaintenanceHistory");
            migrationBuilder.DropColumn(name: "NextMaintenanceOdo", table: "MaintenanceHistory");
            migrationBuilder.DropColumn(name: "PartsCost", table: "MaintenanceHistory");
            migrationBuilder.DropColumn(name: "PartsJson", table: "MaintenanceHistory");
            migrationBuilder.DropColumn(name: "TechnicianId", table: "MaintenanceHistory");
            migrationBuilder.DropColumn(name: "TotalCost", table: "MaintenanceHistory");
            migrationBuilder.DropColumn(name: "IsVerified", table: "Lead");
            migrationBuilder.DropColumn(name: "PricingRulesJson", table: "CarrierPartners");
            migrationBuilder.DropColumn(name: "SlaJson", table: "CarrierPartners");
            migrationBuilder.AlterColumn<int>(
                name: "OutputId",
                table: "PlateDossier",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
            migrationBuilder.CreateTable(
                name: "ContractTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DynamicFields = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTemplates", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "ServiceEvaluation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContactId = table.Column<int>(type: "integer", nullable: false),
                    ServiceBookingId = table.Column<int>(type: "integer", nullable: false),
                    AdminRepliedById = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Criteria = table.Column<string>(type: "text", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DirectReplyText = table.Column<string>(type: "text", nullable: true),
                    InternalNotes = table.Column<string>(type: "text", nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcessingStatus = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Review = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceEvaluation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceEvaluation_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceEvaluation_ServiceBooking_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBooking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "ContractTemplateAuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Details = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OldValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTemplateAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractTemplateAuditLog_ContractTemplates_ContractTemplate~",
                        column: x => x.ContractTemplateId,
                        principalTable: "ContractTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplateAuditLog_ContractTemplateId",
                table: "ContractTemplateAuditLog",
                column: "ContractTemplateId");
            migrationBuilder.CreateIndex(
                name: "IX_ServiceEvaluation_ContactId",
                table: "ServiceEvaluation",
                column: "ContactId");
            migrationBuilder.CreateIndex(
                name: "IX_ServiceEvaluation_ServiceBookingId",
                table: "ServiceEvaluation",
                column: "ServiceBookingId");
            migrationBuilder.AddForeignKey(
                name: "FK_PlateDossier_Output_OutputId",
                table: "PlateDossier",
                column: "OutputId",
                principalTable: "Output",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
