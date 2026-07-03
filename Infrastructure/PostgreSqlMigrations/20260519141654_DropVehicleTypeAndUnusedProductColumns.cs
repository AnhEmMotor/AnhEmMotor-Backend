using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
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
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "ProductVariant",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ProductCategory",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "VehicleTypeId", table: "Product", type: "integer", nullable: true);
            migrationBuilder.CreateTable(
                name: "VehicleType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
