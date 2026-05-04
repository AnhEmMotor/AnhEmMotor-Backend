using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantPrimaryAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "ProductVariant",
                type: "nvarchar(150)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldNullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ColorCode",
                table: "ProductVariant",
                type: "nvarchar(20)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ColorName",
                table: "ProductVariant",
                type: "nvarchar(100)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "SKU",
                table: "ProductVariant",
                type: "nvarchar(50)",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "VersionName",
                table: "ProductVariant",
                type: "nvarchar(100)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ColorCode", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "ColorName", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "SKU", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "VersionName", table: "ProductVariant");
            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "ProductVariant",
                type: "nvarchar(1000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldNullable: true);
        }
    }
}
