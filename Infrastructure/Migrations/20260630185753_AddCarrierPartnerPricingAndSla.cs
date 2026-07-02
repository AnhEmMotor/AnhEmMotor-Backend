using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCarrierPartnerPricingAndSla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "ParcelDeliveryOrders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReturnShippingCost",
                table: "ParcelDeliveryOrders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PricingRulesJson",
                table: "CarrierPartners",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlaJson",
                table: "CarrierPartners",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "ParcelDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "ReturnShippingCost",
                table: "ParcelDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "PricingRulesJson",
                table: "CarrierPartners");

            migrationBuilder.DropColumn(
                name: "SlaJson",
                table: "CarrierPartners");
        }
    }
}
