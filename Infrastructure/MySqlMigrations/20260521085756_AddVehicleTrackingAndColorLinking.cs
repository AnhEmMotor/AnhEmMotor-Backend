using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class AddVehicleTrackingAndColorLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Vehicle_Lead_LeadId", table: "Vehicle");
            migrationBuilder.DropColumn(name: "ColorCode", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "ColorName", table: "ProductVariant");
            migrationBuilder.DropColumn(name: "CategoryGroup", table: "ProductCategory");
            migrationBuilder.RenameColumn(name: "VersionName", table: "ProductVariant", newName: "VariantName");
            migrationBuilder.AlterColumn<int>(
                name: "LeadId",
                table: "Vehicle",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.AddColumn<int>(name: "InputInfoId", table: "Vehicle", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "OutputInfoId", table: "Vehicle", type: "int", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ManagementType",
                table: "ProductCategory",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<int>(
                name: "ProductVariantColorId",
                table: "OutputInfo",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "ProductVariantColorId",
                table: "InputInfo",
                type: "int",
                nullable: true);
            migrationBuilder.CreateTable(
                name: "ProductVariantColor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ColorName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ColorCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoverImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariantColor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariantColor_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateIndex(name: "IX_Vehicle_InputInfoId", table: "Vehicle", column: "InputInfoId");
            migrationBuilder.CreateIndex(name: "IX_Vehicle_OutputInfoId", table: "Vehicle", column: "OutputInfoId");
            migrationBuilder.CreateIndex(
                name: "IX_OutputInfo_ProductVariantColorId",
                table: "OutputInfo",
                column: "ProductVariantColorId");
            migrationBuilder.CreateIndex(
                name: "IX_InputInfo_ProductVariantColorId",
                table: "InputInfo",
                column: "ProductVariantColorId");
            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantColor_ProductVariantId",
                table: "ProductVariantColor",
                column: "ProductVariantId");
            migrationBuilder.AddForeignKey(
                name: "FK_InputInfo_ProductVariantColor_ProductVariantColorId",
                table: "InputInfo",
                column: "ProductVariantColorId",
                principalTable: "ProductVariantColor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_OutputInfo_ProductVariantColor_ProductVariantColorId",
                table: "OutputInfo",
                column: "ProductVariantColorId",
                principalTable: "ProductVariantColor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_InputInfo_InputInfoId",
                table: "Vehicle",
                column: "InputInfoId",
                principalTable: "InputInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Lead_LeadId",
                table: "Vehicle",
                column: "LeadId",
                principalTable: "Lead",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_OutputInfo_OutputInfoId",
                table: "Vehicle",
                column: "OutputInfoId",
                principalTable: "OutputInfo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InputInfo_ProductVariantColor_ProductVariantColorId",
                table: "InputInfo");
            migrationBuilder.DropForeignKey(
                name: "FK_OutputInfo_ProductVariantColor_ProductVariantColorId",
                table: "OutputInfo");
            migrationBuilder.DropForeignKey(name: "FK_Vehicle_InputInfo_InputInfoId", table: "Vehicle");
            migrationBuilder.DropForeignKey(name: "FK_Vehicle_Lead_LeadId", table: "Vehicle");
            migrationBuilder.DropForeignKey(name: "FK_Vehicle_OutputInfo_OutputInfoId", table: "Vehicle");
            migrationBuilder.DropTable(name: "ProductVariantColor");
            migrationBuilder.DropIndex(name: "IX_Vehicle_InputInfoId", table: "Vehicle");
            migrationBuilder.DropIndex(name: "IX_Vehicle_OutputInfoId", table: "Vehicle");
            migrationBuilder.DropIndex(name: "IX_OutputInfo_ProductVariantColorId", table: "OutputInfo");
            migrationBuilder.DropIndex(name: "IX_InputInfo_ProductVariantColorId", table: "InputInfo");
            migrationBuilder.DropColumn(name: "InputInfoId", table: "Vehicle");
            migrationBuilder.DropColumn(name: "OutputInfoId", table: "Vehicle");
            migrationBuilder.DropColumn(name: "ManagementType", table: "ProductCategory");
            migrationBuilder.DropColumn(name: "ProductVariantColorId", table: "OutputInfo");
            migrationBuilder.DropColumn(name: "ProductVariantColorId", table: "InputInfo");
            migrationBuilder.RenameColumn(name: "VariantName", table: "ProductVariant", newName: "VersionName");
            migrationBuilder.AlterColumn<int>(
                name: "LeadId",
                table: "Vehicle",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ColorCode",
                table: "ProductVariant",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "ColorName",
                table: "ProductVariant",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "CategoryGroup",
                table: "ProductCategory",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Lead_LeadId",
                table: "Vehicle",
                column: "LeadId",
                principalTable: "Lead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
