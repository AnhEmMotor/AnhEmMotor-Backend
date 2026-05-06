using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductCategorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductCategory",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProductCategory",
                type: "bit",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "ProductCategory",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ProductCategory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ImageUrl", table: "ProductCategory");
            migrationBuilder.DropColumn(name: "IsActive", table: "ProductCategory");
            migrationBuilder.DropColumn(name: "Slug", table: "ProductCategory");
            migrationBuilder.DropColumn(name: "SortOrder", table: "ProductCategory");
        }
    }
}
