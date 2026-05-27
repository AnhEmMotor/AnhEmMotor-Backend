using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropVehicleTypeAndUnusedProductColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Product_VehicleType_VehicleTypeId", table: "Product");
            migrationBuilder.DropTable(name: "VehicleType");
            migrationBuilder.DropIndex(name: "IX_Product_VehicleTypeId", table: "Product");
            migrationBuilder.DropColumn(name: "StockQuantity", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "SortOrder", table: "ProductCategory");
            migrationBuilder.DropColumn(name: "VehicleTypeId", table: "Product");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "StockQuantity", table: "ProductVariant", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ProductCategory",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "VehicleTypeId", table: "Product", type: "int", nullable: true);
            migrationBuilder.CreateTable(
                name: "VehicleType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleType", x => x.Id);
                });
            migrationBuilder.CreateIndex(name: "IX_Product_VehicleTypeId", table: "Product", column: "VehicleTypeId");
            migrationBuilder.AddForeignKey(
                name: "FK_Product_VehicleType_VehicleTypeId",
                table: "Product",
                column: "VehicleTypeId",
                principalTable: "VehicleType",
                principalColumn: "Id");
        }
    }
}
