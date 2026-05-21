using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierTypeIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PartnerTypeId",
                table: "Supplier",
                type: "nvarchar(50)",
                nullable: true);
            migrationBuilder.CreateTable(
                name: "PartnerType",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerType", x => x.Key);
                });
            migrationBuilder.InsertData(
                table: "PartnerType",
                columns: new[] { "Key", "CreatedAt", "DeletedAt", "UpdatedAt" },
                values: new object[,]
                {
                {
                    "financial",
                    null,
                    null,
                    null
                },
                {
                    "insurance",
                    null,
                    null,
                    null
                },
                {
                    "supplier",
                    null,
                    null,
                    null
                }
                });
            migrationBuilder.CreateIndex(name: "IX_Supplier_PartnerTypeId", table: "Supplier", column: "PartnerTypeId");
            migrationBuilder.AddForeignKey(
                name: "FK_Supplier_PartnerType_PartnerTypeId",
                table: "Supplier",
                column: "PartnerTypeId",
                principalTable: "PartnerType",
                principalColumn: "Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Supplier_PartnerType_PartnerTypeId", table: "Supplier");
            migrationBuilder.DropTable(name: "PartnerType");
            migrationBuilder.DropIndex(name: "IX_Supplier_PartnerTypeId", table: "Supplier");
            migrationBuilder.DropColumn(name: "PartnerTypeId", table: "Supplier");
        }
    }
}
