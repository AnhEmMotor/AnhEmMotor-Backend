using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SupportMultiColorFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCollectionPhoto_ProductVariant_ProductVariantId",
                table: "ProductCollectionPhoto");
            migrationBuilder.DropForeignKey(
                name: "FK_VariantOptionValue_ProductVariant_VariantId",
                table: "VariantOptionValue");
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "VariantOptionValue",
                type: "datetimeoffset",
                nullable: true);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "VariantOptionValue",
                type: "datetimeoffset",
                nullable: true);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "VariantOptionValue",
                type: "datetimeoffset",
                nullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "ProductVariant",
                type: "nvarchar(1000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "ColorName",
                table: "ProductVariant",
                type: "nvarchar(500)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "ProductVariant",
                type: "nvarchar(200)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldNullable: true);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ProductCollectionPhoto",
                type: "datetimeoffset",
                nullable: true);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "ProductCollectionPhoto",
                type: "datetimeoffset",
                nullable: true);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ProductCollectionPhoto",
                type: "datetimeoffset",
                nullable: true);
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCollectionPhoto_ProductVariant_ProductVariantId",
                table: "ProductCollectionPhoto");
            migrationBuilder.DropForeignKey(
                name: "FK_VariantOptionValue_ProductVariant_VariantId",
                table: "VariantOptionValue");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "VariantOptionValue");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "VariantOptionValue");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "VariantOptionValue");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "ProductCollectionPhoto");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "ProductCollectionPhoto");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "ProductCollectionPhoto");
            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "ProductVariant",
                type: "nvarchar(150)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "ColorName",
                table: "ProductVariant",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "ProductVariant",
                type: "nvarchar(20)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldNullable: true);
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
    }
}
