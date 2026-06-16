using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDBContext))]
    [Migration("20260603070000_AddVehicleIsActiveAndProductId")]
    public partial class AddVehicleIsActiveAndProductId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Vehicle",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Vehicle",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_ProductId",
                table: "Vehicle",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Product_ProductId",
                table: "Vehicle",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Product_ProductId",
                table: "Vehicle");

            migrationBuilder.DropIndex(
                name: "IX_Vehicle_ProductId",
                table: "Vehicle");

            migrationBuilder.DropColumn(name: "IsActive", table: "Vehicle");
            migrationBuilder.DropColumn(name: "ProductId", table: "Vehicle");
        }
    }
}
