using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class RefactorInputToInventoryReceiptAndPurchaseRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_InputInfo_InputInfoId",
                table: "Vehicle");

            migrationBuilder.DropTable(
                name: "InputInfo");

            migrationBuilder.DropTable(
                name: "Input");

            migrationBuilder.DropTable(
                name: "InputStatus");

            migrationBuilder.DropColumn(
                name: "Highlights",
                table: "Product");

            migrationBuilder.RenameColumn(
                name: "InputInfoId",
                table: "Vehicle",
                newName: "InventoryReceiptInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicle_InputInfoId",
                table: "Vehicle",
                newName: "IX_Vehicle_InventoryReceiptInfoId");

            migrationBuilder.CreateTable(
                name: "InventoryReceiptStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptStatus", x => x.Key);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PurchaseRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Status = table.Column<string>(type: "varchar(30)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ApprovedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequest_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseRequest_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventoryReceipt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InventoryReceiptDate = table.Column<long>(type: "bigint", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PurchaseRequestId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ConfirmedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    SourceOrderId = table.Column<int>(type: "int", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_InventoryReceiptStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "InventoryReceiptStatus",
                        principalColumn: "Key");
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_Output_SourceOrderId",
                        column: x => x.SourceOrderId,
                        principalTable: "Output",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_PurchaseRequest_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PurchaseRequestItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PurchaseRequestId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequestItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItem_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItem_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItem_PurchaseRequest_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventoryReceiptInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InventoryReceiptId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: true),
                    RemainingCount = table.Column<int>(type: "int", nullable: true),
                    ParentOutputInfoId = table.Column<int>(type: "int", nullable: true),
                    PurchaseRequestItemId = table.Column<int>(type: "int", nullable: true),
                    QuotationProductRowId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfo_InventoryReceipt_InventoryReceiptId",
                        column: x => x.InventoryReceiptId,
                        principalTable: "InventoryReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfo_OutputInfo_ParentOutputInfoId",
                        column: x => x.ParentOutputInfoId,
                        principalTable: "OutputInfo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfo_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfo_PurchaseRequestItem_PurchaseRequestItem~",
                        column: x => x.PurchaseRequestItemId,
                        principalTable: "PurchaseRequestItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfo_QuotationProductRows_QuotationProductRo~",
                        column: x => x.QuotationProductRowId,
                        principalTable: "QuotationProductRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_ConfirmedBy",
                table: "InventoryReceipt",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_CreatedBy",
                table: "InventoryReceipt",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_PurchaseRequestId",
                table: "InventoryReceipt",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_SourceOrderId",
                table: "InventoryReceipt",
                column: "SourceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_StatusId",
                table: "InventoryReceipt",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_SupplierId",
                table: "InventoryReceipt",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfo_InventoryReceiptId",
                table: "InventoryReceiptInfo",
                column: "InventoryReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfo_ParentOutputInfoId",
                table: "InventoryReceiptInfo",
                column: "ParentOutputInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfo_ProductVariantId",
                table: "InventoryReceiptInfo",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfo_PurchaseRequestItemId",
                table: "InventoryReceiptInfo",
                column: "PurchaseRequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfo_QuotationProductRowId",
                table: "InventoryReceiptInfo",
                column: "QuotationProductRowId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequest_ApprovedBy",
                table: "PurchaseRequest",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequest_CreatedBy",
                table: "PurchaseRequest",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_ProductVariantColorId",
                table: "PurchaseRequestItem",
                column: "ProductVariantColorId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_ProductVariantId",
                table: "PurchaseRequestItem",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_PurchaseRequestId",
                table: "PurchaseRequestItem",
                column: "PurchaseRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_InventoryReceiptInfo_InventoryReceiptInfoId",
                table: "Vehicle",
                column: "InventoryReceiptInfoId",
                principalTable: "InventoryReceiptInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_InventoryReceiptInfo_InventoryReceiptInfoId",
                table: "Vehicle");

            migrationBuilder.DropTable(
                name: "InventoryReceiptInfo");

            migrationBuilder.DropTable(
                name: "InventoryReceipt");

            migrationBuilder.DropTable(
                name: "PurchaseRequestItem");

            migrationBuilder.DropTable(
                name: "InventoryReceiptStatus");

            migrationBuilder.DropTable(
                name: "PurchaseRequest");

            migrationBuilder.RenameColumn(
                name: "InventoryReceiptInfoId",
                table: "Vehicle",
                newName: "InputInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicle_InventoryReceiptInfoId",
                table: "Vehicle",
                newName: "IX_Vehicle_InputInfoId");

            migrationBuilder.AddColumn<string>(
                name: "Highlights",
                table: "Product",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InputStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputStatus", x => x.Key);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Input",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ConfirmedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    SourceOrderId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true),
                    InputDate = table.Column<long>(type: "bigint", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Input", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Input_InputStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "InputStatus",
                        principalColumn: "Key");
                    table.ForeignKey(
                        name: "FK_Input_Output_SourceOrderId",
                        column: x => x.SourceOrderId,
                        principalTable: "Output",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Input_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Input_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Input_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InputInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InputId = table.Column<int>(type: "int", nullable: false),
                    ParentOutputInfoId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    Count = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true),
                    InputPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RemainingCount = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputInfo_Input_InputId",
                        column: x => x.InputId,
                        principalTable: "Input",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InputInfo_OutputInfo_ParentOutputInfoId",
                        column: x => x.ParentOutputInfoId,
                        principalTable: "OutputInfo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_InputInfo_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InputInfo_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Input_ConfirmedBy",
                table: "Input",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Input_CreatedBy",
                table: "Input",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Input_SourceOrderId",
                table: "Input",
                column: "SourceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Input_StatusId",
                table: "Input",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Input_SupplierId",
                table: "Input",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_InputInfo_InputId",
                table: "InputInfo",
                column: "InputId");

            migrationBuilder.CreateIndex(
                name: "IX_InputInfo_ParentOutputInfoId",
                table: "InputInfo",
                column: "ParentOutputInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_InputInfo_ProductVariantColorId",
                table: "InputInfo",
                column: "ProductVariantColorId");

            migrationBuilder.CreateIndex(
                name: "IX_InputInfo_ProductVariantId",
                table: "InputInfo",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_InputInfo_InputInfoId",
                table: "Vehicle",
                column: "InputInfoId",
                principalTable: "InputInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
