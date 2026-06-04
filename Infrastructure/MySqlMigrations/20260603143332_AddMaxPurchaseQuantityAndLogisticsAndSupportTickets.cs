using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class AddMaxPurchaseQuantityAndLogisticsAndSupportTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Product_ProductId",
                table: "Vehicle");

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
                name: "ServiceBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppointmentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AppointmentTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedSaleId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CustomerNote = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdminNote = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CancelledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancellationReason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceBookings_Users_AssignedSaleId",
                        column: x => x.AssignedSaleId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceBookings_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_ServiceBookings_AssignedSaleId",
                table: "ServiceBookings",
                column: "AssignedSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_VehicleId",
                table: "ServiceBookings",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_AssignedAdminId",
                table: "SupportTicket",
                column: "AssignedAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_CustomerId",
                table: "SupportTicket",
                column: "CustomerId");

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
                name: "ServiceBookings");

            migrationBuilder.DropTable(
                name: "CustomerContact");

            migrationBuilder.DropTable(
                name: "SupportTicket");

            migrationBuilder.DropIndex(
                name: "IX_Vehicle_UserId",
                table: "Vehicle");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Product_ProductId",
                table: "Vehicle",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");
        }
    }
}
