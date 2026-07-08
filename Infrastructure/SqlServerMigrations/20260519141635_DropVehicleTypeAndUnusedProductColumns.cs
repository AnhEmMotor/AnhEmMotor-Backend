using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class DropVehicleTypeAndUnusedProductColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('FK_Product_VehicleType_VehicleTypeId', 'F') IS NOT NULL
    ALTER TABLE [Product] DROP CONSTRAINT [FK_Product_VehicleType_VehicleTypeId]
");
            migrationBuilder.Sql(@"
IF OBJECT_ID('VehicleType', 'U') IS NOT NULL
    DROP TABLE [VehicleType]
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Product_VehicleTypeId' AND object_id = OBJECT_ID('Product'))
    DROP INDEX [IX_Product_VehicleTypeId] ON [Product]
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'StockQuantity' AND object_id = OBJECT_ID('ProductVariant'))
    ALTER TABLE [ProductVariant] DROP COLUMN [StockQuantity]
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'SortOrder' AND object_id = OBJECT_ID('ProductCategory'))
    ALTER TABLE [ProductCategory] DROP COLUMN [SortOrder]
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'VehicleTypeId' AND object_id = OBJECT_ID('Product'))
    ALTER TABLE [Product] DROP COLUMN [VehicleTypeId]
");
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
