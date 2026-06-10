using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BoxCondition",
                table: "ParcelDeliveryOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductCondition",
                table: "ParcelDeliveryOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnAction",
                table: "ParcelDeliveryOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnInternalNote",
                table: "ParcelDeliveryOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnProofImage",
                table: "ParcelDeliveryOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnReason",
                table: "ParcelDeliveryOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CarrierPartners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarrierCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebhookSecret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebhookEndpointUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AutoSyncPricing = table.Column<bool>(type: "bit", nullable: false),
                    MaxParcelWeightKg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AllowLiquidCargo = table.Column<bool>(type: "bit", nullable: false),
                    AllowOversizeCargo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarrierPartners", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarrierPartners");

            migrationBuilder.DropColumn(
                name: "BoxCondition",
                table: "ParcelDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "ProductCondition",
                table: "ParcelDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "ReturnAction",
                table: "ParcelDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "ReturnInternalNote",
                table: "ParcelDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "ReturnProofImage",
                table: "ParcelDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "ReturnReason",
                table: "ParcelDeliveryOrders");
        }
    }
}
