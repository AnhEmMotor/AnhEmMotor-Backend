using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBaseEntityFromMappingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CreatedAt", table: "VariantOptionValue");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "VariantOptionValue");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "VariantOptionValue");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "ProductCollectionPhoto");
            migrationBuilder.DropColumn(name: "DeletedAt", table: "ProductCollectionPhoto");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "ProductCollectionPhoto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
