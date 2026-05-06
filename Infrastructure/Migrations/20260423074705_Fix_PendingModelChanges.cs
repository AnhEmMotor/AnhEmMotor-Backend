using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EngineType",
                table: "ProductVariant",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrontBrake",
                table: "ProductVariant",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrontSuspension",
                table: "ProductVariant",
                type: "nvarchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RearBrake",
                table: "ProductVariant",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RearSuspension",
                table: "ProductVariant",
                type: "nvarchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "ProductVariant",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origin",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherStandards",
                table: "Product",
                type: "nvarchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StdDot",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StdEce",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StdJis",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StdSnell",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Product",
                type: "nvarchar(20)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarrantyPeriod",
                table: "Product",
                type: "nvarchar(50)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EngineType",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "FrontBrake",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "FrontSuspension",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "RearBrake",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "RearSuspension",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "OtherStandards",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "StdDot",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "StdEce",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "StdJis",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "StdSnell",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "WarrantyPeriod",
                table: "Product");
        }
    }
}
