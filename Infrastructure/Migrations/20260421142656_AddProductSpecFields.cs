using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSpecFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BatteryType",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "DashboardType",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "FrameType",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "FrontBrake",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "FrontTireSize",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "FuelSystem",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "LightingSystem",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "RearBrake",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "RearTireSize",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "BatteryType", table: "Product");
            migrationBuilder.DropColumn(name: "DashboardType", table: "Product");
            migrationBuilder.DropColumn(name: "FrameType", table: "Product");
            migrationBuilder.DropColumn(name: "FrontBrake", table: "Product");
            migrationBuilder.DropColumn(name: "FrontTireSize", table: "Product");
            migrationBuilder.DropColumn(name: "FuelSystem", table: "Product");
            migrationBuilder.DropColumn(name: "LightingSystem", table: "Product");
            migrationBuilder.DropColumn(name: "RearBrake", table: "Product");
            migrationBuilder.DropColumn(name: "RearTireSize", table: "Product");
        }
    }
}
