using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductSchema_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dimensions",
                table: "ProductVariant",
                type: "nvarchar(35)",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "FuelCapacity",
                table: "ProductVariant",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "GroundClearance",
                table: "ProductVariant",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "SeatHeight",
                table: "ProductVariant",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "TireSize",
                table: "ProductVariant",
                type: "nvarchar(100)",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "ProductVariant",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "Wheelbase",
                table: "ProductVariant",
                type: "decimal(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<int>(name: "ParentId", table: "ProductCategory", type: "int", nullable: true);
            migrationBuilder.AddColumn<string>(name: "LogoUrl", table: "Brand", type: "nvarchar(1000)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Origin", table: "Brand", type: "nvarchar(100)", nullable: true);
            migrationBuilder.CreateTable(
                name: "ProductCompatibility",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    BaseProductId = table.Column<int>(type: "int", nullable: false),
                    CompatibleVehicleModelId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCompatibility", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCompatibility_Product_BaseProductId",
                        column: x => x.BaseProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCompatibility_Product_CompatibleVehicleModelId",
                        column: x => x.CompatibleVehicleModelId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateIndex(
                name: "IX_ProductCategory_ParentId",
                table: "ProductCategory",
                column: "ParentId");
            migrationBuilder.CreateIndex(
                name: "IX_ProductCompatibility_BaseProductId",
                table: "ProductCompatibility",
                column: "BaseProductId");
            migrationBuilder.CreateIndex(
                name: "IX_ProductCompatibility_CompatibleVehicleModelId",
                table: "ProductCompatibility",
                column: "CompatibleVehicleModelId");
            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategory_ProductCategory_ParentId",
                table: "ProductCategory",
                column: "ParentId",
                principalTable: "ProductCategory",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategory_ProductCategory_ParentId",
                table: "ProductCategory");
            migrationBuilder.DropTable(name: "ProductCompatibility");
            migrationBuilder.DropIndex(name: "IX_ProductCategory_ParentId", table: "ProductCategory");
            migrationBuilder.DropColumn(name: "Dimensions", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "FuelCapacity", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "GroundClearance", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "SeatHeight", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "TireSize", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "Weight", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "Wheelbase", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "ParentId", table: "ProductCategory");
            migrationBuilder.DropColumn(name: "LogoUrl", table: "Brand");
            migrationBuilder.DropColumn(name: "Origin", table: "Brand");
        }
    }
}
