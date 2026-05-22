using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProductVariantNamingAndAddVehicleColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_InputInfo_ProductVariant_ProductId", table: "InputInfo");
            migrationBuilder.DropForeignKey(name: "FK_OutputInfo_ProductVariant_ProductVarientId", table: "OutputInfo");
            migrationBuilder.RenameColumn(name: "ProductVarientId", table: "OutputInfo", newName: "ProductVariantId");
            migrationBuilder.RenameIndex(
                name: "IX_OutputInfo_ProductVarientId",
                table: "OutputInfo",
                newName: "IX_OutputInfo_ProductVariantId");
            migrationBuilder.RenameColumn(name: "ProductId", table: "InputInfo", newName: "ProductVariantId");
            migrationBuilder.RenameIndex(
                name: "IX_InputInfo_ProductId",
                table: "InputInfo",
                newName: "IX_InputInfo_ProductVariantId");
            migrationBuilder.AddColumn<int>(
                name: "ProductVariantColorId",
                table: "Vehicle",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<int>(name: "ProductVariantId", table: "Vehicle", type: "int", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Vehicle",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_ProductVariantColorId",
                table: "Vehicle",
                column: "ProductVariantColorId");
            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_ProductVariantId",
                table: "Vehicle",
                column: "ProductVariantId");
            migrationBuilder.AddForeignKey(
                name: "FK_InputInfo_ProductVariant_ProductVariantId",
                table: "InputInfo",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_OutputInfo_ProductVariant_ProductVariantId",
                table: "OutputInfo",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_ProductVariantColor_ProductVariantColorId",
                table: "Vehicle",
                column: "ProductVariantColorId",
                principalTable: "ProductVariantColor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_ProductVariant_ProductVariantId",
                table: "Vehicle",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_InputInfo_ProductVariant_ProductVariantId", table: "InputInfo");
            migrationBuilder.DropForeignKey(name: "FK_OutputInfo_ProductVariant_ProductVariantId", table: "OutputInfo");
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_ProductVariantColor_ProductVariantColorId",
                table: "Vehicle");
            migrationBuilder.DropForeignKey(name: "FK_Vehicle_ProductVariant_ProductVariantId", table: "Vehicle");
            migrationBuilder.DropIndex(name: "IX_Vehicle_ProductVariantColorId", table: "Vehicle");
            migrationBuilder.DropIndex(name: "IX_Vehicle_ProductVariantId", table: "Vehicle");
            migrationBuilder.DropColumn(name: "ProductVariantColorId", table: "Vehicle");
            migrationBuilder.DropColumn(name: "ProductVariantId", table: "Vehicle");
            migrationBuilder.DropColumn(name: "Status", table: "Vehicle");
            migrationBuilder.RenameColumn(name: "ProductVariantId", table: "OutputInfo", newName: "ProductVarientId");
            migrationBuilder.RenameIndex(
                name: "IX_OutputInfo_ProductVariantId",
                table: "OutputInfo",
                newName: "IX_OutputInfo_ProductVarientId");
            migrationBuilder.RenameColumn(name: "ProductVariantId", table: "InputInfo", newName: "ProductId");
            migrationBuilder.RenameIndex(
                name: "IX_InputInfo_ProductVariantId",
                table: "InputInfo",
                newName: "IX_InputInfo_ProductId");
            migrationBuilder.AddForeignKey(
                name: "FK_InputInfo_ProductVariant_ProductId",
                table: "InputInfo",
                column: "ProductId",
                principalTable: "ProductVariant",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_OutputInfo_ProductVariant_ProductVarientId",
                table: "OutputInfo",
                column: "ProductVarientId",
                principalTable: "ProductVariant",
                principalColumn: "Id");
        }
    }
}
