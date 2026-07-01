using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceEvaluation");

            migrationBuilder.CreateTable(
                name: "ConversionTool",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    DelaySeconds = table.Column<int>(type: "int", nullable: true),
                    Pages = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Views = table.Column<int>(type: "int", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    Leads = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversionTool", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReturnRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalTrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Carrier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvidenceImagesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                name: "WorkshopPayment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentNumber = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    VehicleInfo = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    ServiceDescription = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    ReceivedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    InvoicePrintedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopPayment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReturnRequestItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnRequestId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReturnQuantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequest_OrderId",
                table: "ReturnRequest",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequestItem_ProductId",
                table: "ReturnRequestItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequestItem_ReturnRequestId",
                table: "ReturnRequestItem",
                column: "ReturnRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversionTool");

            migrationBuilder.DropTable(
                name: "ReturnRequestItem");

            migrationBuilder.DropTable(
                name: "WorkshopPayment");

            migrationBuilder.DropTable(
                name: "ReturnRequest");

            migrationBuilder.CreateTable(
                name: "ServiceEvaluation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    AdminRepliedById = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Criteria = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DirectReplyText = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ProcessingStatus = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Review = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEvaluation_ContactId",
                table: "ServiceEvaluation",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEvaluation_ServiceBookingId",
                table: "ServiceEvaluation",
                column: "ServiceBookingId");
        }
    }
}
