using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParcelDeliveryOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerAddress",
                table: "ParcelDeliveryOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "ParcelDeliveryOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalOrderCode",
                table: "ParcelDeliveryOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ParcelDeliveryOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParcelDeliveryOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShelfLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsPicked = table.Column<bool>(type: "bit", nullable: false),
                    IsRestricted = table.Column<bool>(type: "bit", nullable: false),
                    IsOutOfStock = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelDeliveryOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelDeliveryOrderItems_ParcelDeliveryOrders_ParcelDeliveryOrderId",
                        column: x => x.ParcelDeliveryOrderId,
                        principalTable: "ParcelDeliveryOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDeliveryOrderItems_ParcelDeliveryOrderId",
                table: "ParcelDeliveryOrderItems",
                column: "ParcelDeliveryOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelDeliveryOrderItems");

            migrationBuilder.DropColumn(
                name: "CustomerAddress",
                table: "ParcelDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "ParcelDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "OriginalOrderCode",
                table: "ParcelDeliveryOrders");
        }
    }
}
