using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotAllowNullInProductCollectionPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCollectionPhoto_ProductVariant_ProductVariantId",
                table: "ProductCollectionPhoto");

            migrationBuilder.AlterColumn<int>(
                name: "ProductVariantId",
                table: "ProductCollectionPhoto",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCollectionPhoto_ProductVariant_ProductVariantId",
                table: "ProductCollectionPhoto",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCollectionPhoto_ProductVariant_ProductVariantId",
                table: "ProductCollectionPhoto");

            migrationBuilder.AlterColumn<int>(
                name: "ProductVariantId",
                table: "ProductCollectionPhoto",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCollectionPhoto_ProductVariant_ProductVariantId",
                table: "ProductCollectionPhoto",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id");
        }
    }
}
