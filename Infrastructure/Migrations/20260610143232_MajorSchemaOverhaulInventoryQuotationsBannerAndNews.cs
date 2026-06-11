using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MajorSchemaOverhaulInventoryQuotationsBannerAndNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_InventoryReceipt_Supplier_SupplierId", table: "InventoryReceipt");
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptInfo_QuotationProductRows_QuotationProductRowId",
                table: "InventoryReceiptInfo");
            migrationBuilder.DropTable(name: "QuotationProductRows");
            migrationBuilder.DropTable(name: "Quotations");
            migrationBuilder.DropIndex(name: "IX_InventoryReceipt_SupplierId", table: "InventoryReceipt");
            migrationBuilder.DropColumn(name: "SupplierId", table: "InventoryReceipt");
            migrationBuilder.DropColumn(name: "ClickCount", table: "Banner");
            migrationBuilder.DropColumn(name: "DisplayOrder", table: "Banner");
            migrationBuilder.DropColumn(name: "EndDate", table: "Banner");
            migrationBuilder.DropColumn(name: "IsActive", table: "Banner");
            migrationBuilder.DropColumn(name: "Position", table: "Banner");
            migrationBuilder.DropColumn(name: "Priority", table: "Banner");
            migrationBuilder.DropColumn(name: "StartDate", table: "Banner");
            migrationBuilder.DropColumn(name: "ViewCount", table: "Banner");
            migrationBuilder.RenameColumn(
                name: "QuotationProductRowId",
                table: "InventoryReceiptInfo",
                newName: "SupplierId");
            migrationBuilder.RenameIndex(
                name: "IX_InventoryReceiptInfo_QuotationProductRowId",
                table: "InventoryReceiptInfo",
                newName: "IX_InventoryReceiptInfo_SupplierId");
            migrationBuilder.RenameColumn(name: "LinkUrl", table: "Banner", newName: "MobileImageUrl");
            migrationBuilder.RenameColumn(name: "ImageUrl", table: "Banner", newName: "DesktopImageUrl");
            migrationBuilder.RenameColumn(name: "CtaText", table: "Banner", newName: "CtaLabel");
            migrationBuilder.AddColumn<decimal>(
                name: "ImportPrice",
                table: "Vehicle",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PurchaseRequestItem",
                type: "datetimeoffset",
                nullable: true);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "PurchaseRequestItem",
                type: "datetimeoffset",
                nullable: true);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PurchaseRequestItem",
                type: "datetimeoffset",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "RejectedBy",
                table: "PurchaseRequest",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "SentBy",
                table: "PurchaseRequest",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "InventoryReceiptInfo",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "InventoryReceipt",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "RejectedBy",
                table: "InventoryReceipt",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "SentBy",
                table: "InventoryReceipt",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<string>(name: "CtaLink", table: "Banner", type: "nvarchar(500)", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Banner",
                type: "nvarchar(1000)",
                nullable: true);
            migrationBuilder.CreateTable(
                name: "InventoryLedger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    TransactionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DocumentCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    PartnerName = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    ImportQty = table.Column<int>(type: "int", nullable: false),
                    ExportQty = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockAfter = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLedger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryLedger_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryLedger_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "InventoryOnHand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    StockQty = table.Column<int>(type: "int", nullable: false),
                    ImportedQty = table.Column<int>(type: "int", nullable: false),
                    ExportedQty = table.Column<int>(type: "int", nullable: false),
                    OrderedQty = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryOnHand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryOnHand_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryOnHand_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "NewsComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    NewsId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    AuthorEmail = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsComments_News_NewsId",
                        column: x => x.NewsId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });
            migrationBuilder.CreateTable(
                name: "NewsProduct",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    NewsId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsProduct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsProduct_News_NewsId",
                        column: x => x.NewsId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsProduct_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NewsProduct_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "ProductQuotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    QuotePrice = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductQuotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductQuotations_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductQuotations_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductQuotations_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id");
                });
            migrationBuilder.CreateTable(
                name: "SupplierDebt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    InventoryReceiptId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDebt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDebt_InventoryReceipt_InventoryReceiptId",
                        column: x => x.InventoryReceiptId,
                        principalTable: "InventoryReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierDebt_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequest_RejectedBy",
                table: "PurchaseRequest",
                column: "RejectedBy");
            migrationBuilder.CreateIndex(name: "IX_PurchaseRequest_SentBy", table: "PurchaseRequest", column: "SentBy");
            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_ApprovedBy",
                table: "InventoryReceipt",
                column: "ApprovedBy");
            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_RejectedBy",
                table: "InventoryReceipt",
                column: "RejectedBy");
            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_SentBy",
                table: "InventoryReceipt",
                column: "SentBy");
            migrationBuilder.CreateIndex(
                name: "IX_InventoryLedger_ProductVariantColorId",
                table: "InventoryLedger",
                column: "ProductVariantColorId");
            migrationBuilder.CreateIndex(
                name: "IX_InventoryLedger_ProductVariantId",
                table: "InventoryLedger",
                column: "ProductVariantId");
            migrationBuilder.CreateIndex(
                name: "IX_InventoryOnHand_ProductVariantColorId",
                table: "InventoryOnHand",
                column: "ProductVariantColorId");
            migrationBuilder.CreateIndex(
                name: "IX_InventoryOnHand_ProductVariantId",
                table: "InventoryOnHand",
                column: "ProductVariantId");
            migrationBuilder.CreateIndex(name: "IX_NewsComments_NewsId", table: "NewsComments", column: "NewsId");
            migrationBuilder.CreateIndex(name: "IX_NewsComments_UserId", table: "NewsComments", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_NewsProduct_NewsId", table: "NewsProduct", column: "NewsId");
            migrationBuilder.CreateIndex(
                name: "IX_NewsProduct_ProductVariantColorId",
                table: "NewsProduct",
                column: "ProductVariantColorId");
            migrationBuilder.CreateIndex(
                name: "IX_NewsProduct_ProductVariantId",
                table: "NewsProduct",
                column: "ProductVariantId");
            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotations_ProductVariantColorId",
                table: "ProductQuotations",
                column: "ProductVariantColorId");
            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotations_ProductVariantId",
                table: "ProductQuotations",
                column: "ProductVariantId");
            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotations_SupplierId",
                table: "ProductQuotations",
                column: "SupplierId");
            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebt_InventoryReceiptId",
                table: "SupplierDebt",
                column: "InventoryReceiptId");
            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebt_SupplierId",
                table: "SupplierDebt",
                column: "SupplierId");
            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceipt_Users_ApprovedBy",
                table: "InventoryReceipt",
                column: "ApprovedBy",
                principalTable: "Users",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceipt_Users_RejectedBy",
                table: "InventoryReceipt",
                column: "RejectedBy",
                principalTable: "Users",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceipt_Users_SentBy",
                table: "InventoryReceipt",
                column: "SentBy",
                principalTable: "Users",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceiptInfo_Supplier_SupplierId",
                table: "InventoryReceiptInfo",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequest_Users_RejectedBy",
                table: "PurchaseRequest",
                column: "RejectedBy",
                principalTable: "Users",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequest_Users_SentBy",
                table: "PurchaseRequest",
                column: "SentBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_InventoryReceipt_Users_ApprovedBy", table: "InventoryReceipt");
            migrationBuilder.DropForeignKey(name: "FK_InventoryReceipt_Users_RejectedBy", table: "InventoryReceipt");
            migrationBuilder.DropForeignKey(name: "FK_InventoryReceipt_Users_SentBy", table: "InventoryReceipt");
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptInfo_Supplier_SupplierId",
                table: "InventoryReceiptInfo");
            migrationBuilder.DropForeignKey(name: "FK_PurchaseRequest_Users_RejectedBy", table: "PurchaseRequest");
            migrationBuilder.DropForeignKey(name: "FK_PurchaseRequest_Users_SentBy", table: "PurchaseRequest");
            migrationBuilder.DropTable(name: "InventoryLedger");
            migrationBuilder.DropTable(name: "InventoryOnHand");
            migrationBuilder.DropTable(name: "NewsComments");
            migrationBuilder.DropTable(name: "NewsProduct");
            migrationBuilder.DropTable(name: "ProductQuotations");
            migrationBuilder.DropTable(name: "SupplierDebt");
            migrationBuilder.DropIndex(name: "IX_PurchaseRequest_RejectedBy", table: "PurchaseRequest");
            migrationBuilder.DropIndex(name: "IX_PurchaseRequest_SentBy", table: "PurchaseRequest");
            migrationBuilder.DropIndex(name: "IX_InventoryReceipt_ApprovedBy", table: "InventoryReceipt");
            migrationBuilder.DropIndex(name: "IX_InventoryReceipt_RejectedBy", table: "InventoryReceipt");
            migrationBuilder.DropIndex(name: "IX_InventoryReceipt_SentBy", table: "InventoryReceipt");
            migrationBuilder.DropColumn(name: "ImportPrice", table: "Vehicle");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "PurchaseRequestItem");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "PurchaseRequestItem");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "PurchaseRequestItem");
            migrationBuilder.DropColumn(name: "RejectedBy", table: "PurchaseRequest");
            migrationBuilder.DropColumn(name: "SentBy", table: "PurchaseRequest");
            migrationBuilder.DropColumn(name: "UnitPrice", table: "InventoryReceiptInfo");
            migrationBuilder.DropColumn(name: "ApprovedBy", table: "InventoryReceipt");
            migrationBuilder.DropColumn(name: "RejectedBy", table: "InventoryReceipt");
            migrationBuilder.DropColumn(name: "SentBy", table: "InventoryReceipt");
            migrationBuilder.DropColumn(name: "CtaLink", table: "Banner");
            migrationBuilder.DropColumn(name: "Description", table: "Banner");
            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "InventoryReceiptInfo",
                newName: "QuotationProductRowId");
            migrationBuilder.RenameIndex(
                name: "IX_InventoryReceiptInfo_SupplierId",
                table: "InventoryReceiptInfo",
                newName: "IX_InventoryReceiptInfo_QuotationProductRowId");
            migrationBuilder.RenameColumn(name: "MobileImageUrl", table: "Banner", newName: "LinkUrl");
            migrationBuilder.RenameColumn(name: "DesktopImageUrl", table: "Banner", newName: "ImageUrl");
            migrationBuilder.RenameColumn(name: "CtaLabel", table: "Banner", newName: "CtaText");
            migrationBuilder.AddColumn<int>(name: "SupplierId", table: "InventoryReceipt", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "ClickCount",
                table: "Banner",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Banner",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndDate",
                table: "Banner",
                type: "datetimeoffset",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Banner",
                type: "bit",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "Position", table: "Banner", type: "nvarchar(50)", nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Banner",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartDate",
                table: "Banner",
                type: "datetimeoffset",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Banner",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Status = table.Column<string>(type: "varchar(30)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotations_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "QuotationProductRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    QuotationId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    QuotePrice = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationProductRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationProductRows_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationProductRows_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationProductRows_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_SupplierId",
                table: "InventoryReceipt",
                column: "SupplierId");
            migrationBuilder.CreateIndex(
                name: "IX_QuotationProductRows_ProductVariantColorId",
                table: "QuotationProductRows",
                column: "ProductVariantColorId");
            migrationBuilder.CreateIndex(
                name: "IX_QuotationProductRows_ProductVariantId",
                table: "QuotationProductRows",
                column: "ProductVariantId");
            migrationBuilder.CreateIndex(
                name: "IX_QuotationProductRows_QuotationId",
                table: "QuotationProductRows",
                column: "QuotationId");
            migrationBuilder.CreateIndex(name: "IX_Quotations_SupplierId", table: "Quotations", column: "SupplierId");
            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceipt_Supplier_SupplierId",
                table: "InventoryReceipt",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceiptInfo_QuotationProductRows_QuotationProductRowId",
                table: "InventoryReceiptInfo",
                column: "QuotationProductRowId",
                principalTable: "QuotationProductRows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
