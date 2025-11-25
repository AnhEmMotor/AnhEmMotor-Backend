using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class RemoveRestrict : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ProductVariant_Product_ProductId", table: "ProductVariant");

            migrationBuilder.DropForeignKey(
                name: "FK_VariantOptionValue_ProductVariant_VariantId",
                table: "VariantOptionValue");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariant_Product_ProductId",
                table: "ProductVariant",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VariantOptionValue_ProductVariant_VariantId",
                table: "VariantOptionValue",
                column: "VariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ProductVariant_Product_ProductId", table: "ProductVariant");

            migrationBuilder.DropForeignKey(
                name: "FK_VariantOptionValue_ProductVariant_VariantId",
                table: "VariantOptionValue");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariant_Product_ProductId",
                table: "ProductVariant",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VariantOptionValue_ProductVariant_VariantId",
                table: "VariantOptionValue",
                column: "VariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
