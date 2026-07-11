using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductBrandLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategoryTranslation_ProductCategory_ProductCategoryId",
                table: "ProductCategoryTranslation");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategoryTranslation_ProductCategory_ProductCategoryId1",
                table: "ProductCategoryTranslation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductCategoryTranslation",
                table: "ProductCategoryTranslation");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategoryTranslation_ProductCategoryId_LanguageCode",
                table: "ProductCategoryTranslation");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategoryTranslation_ProductCategoryId1",
                table: "ProductCategoryTranslation");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId1",
                table: "ProductCategoryTranslation");

            migrationBuilder.RenameTable(
                name: "ProductCategoryTranslation",
                newName: "ProductCategoryTranslations");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductCategoryTranslations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageCode",
                table: "ProductCategoryTranslations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ProductCategoryTranslations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductCategoryTranslations",
                table: "ProductCategoryTranslations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryTranslations_ProductCategoryId",
                table: "ProductCategoryTranslations",
                column: "ProductCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategoryTranslations_ProductCategory_ProductCategoryId",
                table: "ProductCategoryTranslations",
                column: "ProductCategoryId",
                principalTable: "ProductCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategoryTranslations_ProductCategory_ProductCategoryId",
                table: "ProductCategoryTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductCategoryTranslations",
                table: "ProductCategoryTranslations");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategoryTranslations_ProductCategoryId",
                table: "ProductCategoryTranslations");

            migrationBuilder.RenameTable(
                name: "ProductCategoryTranslations",
                newName: "ProductCategoryTranslation");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductCategoryTranslation",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LanguageCode",
                table: "ProductCategoryTranslation",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ProductCategoryTranslation",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductCategoryId1",
                table: "ProductCategoryTranslation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductCategoryTranslation",
                table: "ProductCategoryTranslation",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryTranslation_ProductCategoryId_LanguageCode",
                table: "ProductCategoryTranslation",
                columns: new[] { "ProductCategoryId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryTranslation_ProductCategoryId1",
                table: "ProductCategoryTranslation",
                column: "ProductCategoryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategoryTranslation_ProductCategory_ProductCategoryId",
                table: "ProductCategoryTranslation",
                column: "ProductCategoryId",
                principalTable: "ProductCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategoryTranslation_ProductCategory_ProductCategoryId1",
                table: "ProductCategoryTranslation",
                column: "ProductCategoryId1",
                principalTable: "ProductCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
