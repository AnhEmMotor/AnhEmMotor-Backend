using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
{
    /// <inheritdoc />
    public partial class MajorSchemaOverhaulInventoryQuotationsBannerAndNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceipt_Supplier_SupplierId",
                table: "InventoryReceipt");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptInfo_QuotationProductRows_QuotationProductR~",
                table: "InventoryReceiptInfo");

            migrationBuilder.DropTable(
                name: "QuotationProductRows");

            migrationBuilder.DropTable(
                name: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceipt_SupplierId",
                table: "InventoryReceipt");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "InventoryReceipt");

            migrationBuilder.DropColumn(
                name: "ClickCount",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Banner");

            migrationBuilder.RenameColumn(
                name: "QuotationProductRowId",
                table: "InventoryReceiptInfo",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryReceiptInfo_QuotationProductRowId",
                table: "InventoryReceiptInfo",
                newName: "IX_InventoryReceiptInfo_SupplierId");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "Banner",
                newName: "MobileImageUrl");

            migrationBuilder.RenameColumn(
                name: "LinkUrl",
                table: "Banner",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Banner",
                newName: "DesktopImageUrl");

            migrationBuilder.RenameColumn(
                name: "CtaText",
                table: "Banner",
                newName: "CtaLink");

            migrationBuilder.AddColumn<decimal>(
                name: "ImportPrice",
                table: "Vehicle",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "PurchaseRequestItem",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "PurchaseRequestItem",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "PurchaseRequestItem",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedBy",
                table: "PurchaseRequest",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SentBy",
                table: "PurchaseRequest",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "InventoryReceiptInfo",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "InventoryReceipt",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedBy",
                table: "InventoryReceipt",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SentBy",
                table: "InventoryReceipt",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CtaLabel",
                table: "Banner",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryLedger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransactionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DocumentCode = table.Column<string>(type: "text", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "integer", nullable: true),
                    PartnerName = table.Column<string>(type: "text", nullable: true),
                    ImportQty = table.Column<int>(type: "integer", nullable: false),
                    ExportQty = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    StockAfter = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "integer", nullable: true),
                    StockQty = table.Column<int>(type: "integer", nullable: false),
                    ImportedQty = table.Column<int>(type: "integer", nullable: false),
                    ExportedQty = table.Column<int>(type: "integer", nullable: false),
                    OrderedQty = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NewsId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorName = table.Column<string>(type: "text", nullable: true),
                    AuthorEmail = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NewsId = table.Column<int>(type: "integer", nullable: false),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: true),
                    ProductVariantColorId = table.Column<int>(type: "integer", nullable: true),
                    QuotePrice = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryReceiptId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequest_SentBy",
                table: "PurchaseRequest",
                column: "SentBy");

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

            migrationBuilder.CreateIndex(
                name: "IX_NewsComments_NewsId",
                table: "NewsComments",
                column: "NewsId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsComments_UserId",
                table: "NewsComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsProduct_NewsId",
                table: "NewsProduct",
                column: "NewsId");

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
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceipt_Users_ApprovedBy",
                table: "InventoryReceipt");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceipt_Users_RejectedBy",
                table: "InventoryReceipt");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceipt_Users_SentBy",
                table: "InventoryReceipt");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptInfo_Supplier_SupplierId",
                table: "InventoryReceiptInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequest_Users_RejectedBy",
                table: "PurchaseRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequest_Users_SentBy",
                table: "PurchaseRequest");

            migrationBuilder.DropTable(
                name: "InventoryLedger");

            migrationBuilder.DropTable(
                name: "InventoryOnHand");

            migrationBuilder.DropTable(
                name: "NewsComments");

            migrationBuilder.DropTable(
                name: "NewsProduct");

            migrationBuilder.DropTable(
                name: "ProductQuotations");

            migrationBuilder.DropTable(
                name: "SupplierDebt");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequest_RejectedBy",
                table: "PurchaseRequest");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequest_SentBy",
                table: "PurchaseRequest");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceipt_ApprovedBy",
                table: "InventoryReceipt");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceipt_RejectedBy",
                table: "InventoryReceipt");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceipt_SentBy",
                table: "InventoryReceipt");

            migrationBuilder.DropColumn(
                name: "ImportPrice",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PurchaseRequestItem");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PurchaseRequestItem");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PurchaseRequestItem");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "PurchaseRequest");

            migrationBuilder.DropColumn(
                name: "SentBy",
                table: "PurchaseRequest");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "InventoryReceiptInfo");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "InventoryReceipt");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "InventoryReceipt");

            migrationBuilder.DropColumn(
                name: "SentBy",
                table: "InventoryReceipt");

            migrationBuilder.DropColumn(
                name: "CtaLabel",
                table: "Banner");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "InventoryReceiptInfo",
                newName: "QuotationProductRowId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryReceiptInfo_SupplierId",
                table: "InventoryReceiptInfo",
                newName: "IX_InventoryReceiptInfo_QuotationProductRowId");

            migrationBuilder.RenameColumn(
                name: "MobileImageUrl",
                table: "Banner",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "DesktopImageUrl",
                table: "Banner",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Banner",
                newName: "LinkUrl");

            migrationBuilder.RenameColumn(
                name: "CtaLink",
                table: "Banner",
                newName: "CtaText");

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "InventoryReceipt",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClickCount",
                table: "Banner",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Banner",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndDate",
                table: "Banner",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Banner",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Banner",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartDate",
                table: "Banner",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Banner",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "varchar(30)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductVariantColorId = table.Column<int>(type: "integer", nullable: true),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: true),
                    QuotationId = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    QuotePrice = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationProductRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationProductRows_ProductVariantColor_ProductVariantColo~",
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

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_SupplierId",
                table: "Quotations",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceipt_Supplier_SupplierId",
                table: "InventoryReceipt",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceiptInfo_QuotationProductRows_QuotationProductR~",
                table: "InventoryReceiptInfo",
                column: "QuotationProductRowId",
                principalTable: "QuotationProductRows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
