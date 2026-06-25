using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
{
    /// <inheritdoc />
    public partial class RefactorInputToInventoryReceiptAndPurchaseRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Vehicle_InputInfo_InputInfoId", table: "Vehicle");
            migrationBuilder.DropTable(name: "InputInfo");
            migrationBuilder.DropTable(name: "Input");
            migrationBuilder.DropTable(name: "InputStatus");
            migrationBuilder.DropColumn(name: "Highlights", table: "Product");
            migrationBuilder.RenameColumn(name: "InputInfoId", table: "Vehicle", newName: "InventoryReceiptInfoId");
            migrationBuilder.RenameIndex(
                name: "IX_Vehicle_InputInfoId",
                table: "Vehicle",
                newName: "IX_Vehicle_InventoryReceiptInfoId");
            migrationBuilder.CreateTable(
                name: "InventoryReceiptStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptStatus", x => x.Key);
                });
            migrationBuilder.CreateTable(
                name: "PurchaseRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<string>(type: "varchar(30)", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "InventoryReceipt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryReceiptDate = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    StatusId = table.Column<string>(type: "text", nullable: true),
                    PurchaseRequestId = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceOrderId = table.Column<int>(type: "integer", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "PurchaseRequestItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseRequestId = table.Column<int>(type: "integer", nullable: false),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequestItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItem_ProductVariantColor_ProductVariantColor~",
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
                });
            migrationBuilder.CreateTable(
                name: "InventoryReceiptInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryReceiptId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: true),
                    RemainingCount = table.Column<int>(type: "integer", nullable: true),
                    ParentOutputInfoId = table.Column<int>(type: "integer", nullable: true),
                    PurchaseRequestItemId = table.Column<int>(type: "integer", nullable: true),
                    QuotationProductRowId = table.Column<int>(type: "integer", nullable: true),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                        name: "FK_InventoryReceiptInfo_PurchaseRequestItem_PurchaseRequestIte~",
                        column: x => x.PurchaseRequestItemId,
                        principalTable: "PurchaseRequestItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfo_QuotationProductRows_QuotationProductR~",
                        column: x => x.QuotationProductRowId,
                        principalTable: "QuotationProductRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
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
            migrationBuilder.DropTable(name: "InventoryReceiptInfo");
            migrationBuilder.DropTable(name: "InventoryReceipt");
            migrationBuilder.DropTable(name: "PurchaseRequestItem");
            migrationBuilder.DropTable(name: "InventoryReceiptStatus");
            migrationBuilder.DropTable(name: "PurchaseRequest");
            migrationBuilder.RenameColumn(name: "InventoryReceiptInfoId", table: "Vehicle", newName: "InputInfoId");
            migrationBuilder.RenameIndex(
                name: "IX_Vehicle_InventoryReceiptInfoId",
                table: "Vehicle",
                newName: "IX_Vehicle_InputInfoId");
            migrationBuilder.AddColumn<string>(name: "Highlights", table: "Product", type: "text", nullable: true);
            migrationBuilder.CreateTable(
                name: "InputStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputStatus", x => x.Key);
                });
            migrationBuilder.CreateTable(
                name: "Input",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfirmedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceOrderId = table.Column<int>(type: "integer", nullable: true),
                    StatusId = table.Column<string>(type: "text", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InputDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateTable(
                name: "InputInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InputId = table.Column<int>(type: "integer", nullable: false),
                    ParentOutputInfoId = table.Column<int>(type: "integer", nullable: true),
                    ProductVariantColorId = table.Column<int>(type: "integer", nullable: true),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: true),
                    Count = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InputPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RemainingCount = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });
            migrationBuilder.CreateIndex(name: "IX_Input_ConfirmedBy", table: "Input", column: "ConfirmedBy");
            migrationBuilder.CreateIndex(name: "IX_Input_CreatedBy", table: "Input", column: "CreatedBy");
            migrationBuilder.CreateIndex(name: "IX_Input_SourceOrderId", table: "Input", column: "SourceOrderId");
            migrationBuilder.CreateIndex(name: "IX_Input_StatusId", table: "Input", column: "StatusId");
            migrationBuilder.CreateIndex(name: "IX_Input_SupplierId", table: "Input", column: "SupplierId");
            migrationBuilder.CreateIndex(name: "IX_InputInfo_InputId", table: "InputInfo", column: "InputId");
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
