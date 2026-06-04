using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
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
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ElectronicWarrantyQrCode",
                table: "Vehicle",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMaintenanceDate",
                table: "Vehicle",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextMaintenanceDate",
                table: "Vehicle",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NextMaintenanceOdo",
                table: "Vehicle",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Vehicle",
                type: "uniqueidentifier",
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
                type: "nvarchar(MAX)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Lead",
                type: "nvarchar(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BookingAppointment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    PreferredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreferredTimeSlot = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    AppointmentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Showroom = table.Column<string>(type: "nvarchar(150)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ConfirmedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ProcessedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RepliedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    InternalNote = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                name: "InventoryTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StockBefore = table.Column<int>(type: "int", nullable: false),
                    StockAfter = table.Column<int>(type: "int", nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    PerformedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PerformedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Excerpt = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    SeoTitle = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    SeoDescription = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    SeoKeywords = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PublishedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    BottleneckDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBottleneck = table.Column<bool>(type: "bit", nullable: false),
                    DriverName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DriverPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentLat = table.Column<double>(type: "float", nullable: false),
                    CurrentLng = table.Column<double>(type: "float", nullable: false),
                    EstimatedArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLogistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OutputId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                name: "ServiceBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppointmentTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedSaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "SupportTicket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SLADeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                name: "CustomerContactReply",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    ReplyContent = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false),
                    RepliedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SupportTicketId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
