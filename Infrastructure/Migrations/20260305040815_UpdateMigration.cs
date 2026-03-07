using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class UpdateMigration : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCollectionPhoto_ProductVariant_ProductVariantId",
                table: "ProductCollectionPhoto");

            migrationBuilder.DropForeignKey(
                name: "FK_VariantOptionValue_ProductVariant_VariantId",
                table: "VariantOptionValue");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCollectionPhoto_ProductVariant_ProductVariantId",
                table: "ProductCollectionPhoto",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VariantOptionValue_ProductVariant_VariantId",
                table: "VariantOptionValue",
                column: "VariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCollectionPhoto_ProductVariant_ProductVariantId",
                table: "ProductCollectionPhoto");

            migrationBuilder.DropForeignKey(
                name: "FK_VariantOptionValue_ProductVariant_VariantId",
                table: "VariantOptionValue");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCollectionPhoto_ProductVariant_ProductVariantId",
                table: "ProductCollectionPhoto",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
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
    }
}
