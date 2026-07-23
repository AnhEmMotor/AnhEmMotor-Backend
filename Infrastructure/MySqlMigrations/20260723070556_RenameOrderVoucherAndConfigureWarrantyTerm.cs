using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class RenameOrderVoucherAndConfigureWarrantyTerm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderVoucher_Output_OutputId",
                table: "OrderVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderVoucher_Vouchers_VoucherId",
                table: "OrderVoucher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderVoucher",
                table: "OrderVoucher");

            migrationBuilder.RenameTable(
                name: "OrderVoucher",
                newName: "OrderVouchers");

            migrationBuilder.RenameIndex(
                name: "IX_OrderVoucher_VoucherId",
                table: "OrderVouchers",
                newName: "IX_OrderVouchers_VoucherId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderVoucher_OutputId",
                table: "OrderVouchers",
                newName: "IX_OrderVouchers_OutputId");

            migrationBuilder.UpdateData(
                table: "WarrantyTerm",
                keyColumn: "VehicleType",
                keyValue: null,
                column: "VehicleType",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "VehicleType",
                table: "WarrantyTerm",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "WarrantyTerm",
                keyColumn: "TermName",
                keyValue: null,
                column: "TermName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "TermName",
                table: "WarrantyTerm",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "WarrantyTerm",
                keyColumn: "ErrorCategory",
                keyValue: null,
                column: "ErrorCategory",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorCategory",
                table: "WarrantyTerm",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderVouchers",
                table: "OrderVouchers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyTerm_BrandId",
                table: "WarrantyTerm",
                column: "BrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderVouchers_Output_OutputId",
                table: "OrderVouchers",
                column: "OutputId",
                principalTable: "Output",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderVouchers_Vouchers_VoucherId",
                table: "OrderVouchers",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WarrantyTerm_Brand_BrandId",
                table: "WarrantyTerm",
                column: "BrandId",
                principalTable: "Brand",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderVouchers_Output_OutputId",
                table: "OrderVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderVouchers_Vouchers_VoucherId",
                table: "OrderVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_WarrantyTerm_Brand_BrandId",
                table: "WarrantyTerm");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyTerm_BrandId",
                table: "WarrantyTerm");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderVouchers",
                table: "OrderVouchers");

            migrationBuilder.RenameTable(
                name: "OrderVouchers",
                newName: "OrderVoucher");

            migrationBuilder.RenameIndex(
                name: "IX_OrderVouchers_VoucherId",
                table: "OrderVoucher",
                newName: "IX_OrderVoucher_VoucherId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderVouchers_OutputId",
                table: "OrderVoucher",
                newName: "IX_OrderVoucher_OutputId");

            migrationBuilder.AlterColumn<string>(
                name: "VehicleType",
                table: "WarrantyTerm",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "TermName",
                table: "WarrantyTerm",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorCategory",
                table: "WarrantyTerm",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderVoucher",
                table: "OrderVoucher",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderVoucher_Output_OutputId",
                table: "OrderVoucher",
                column: "OutputId",
                principalTable: "Output",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderVoucher_Vouchers_VoucherId",
                table: "OrderVoucher",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
