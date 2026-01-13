using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class ChangeColumnStructure25122025 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Input_Users_CreatedByUserId", table: "Input");

            migrationBuilder.DropForeignKey(name: "FK_Output_Users_CreatedByUserId", table: "Output");

            migrationBuilder.DropForeignKey(name: "FK_OutputInfo_ProductVariant_ProductId", table: "OutputInfo");

            migrationBuilder.DropColumn(name: "EmpCode", table: "Output");

            migrationBuilder.RenameColumn(name: "ProductId", table: "OutputInfo", newName: "ProductVarientId");

            migrationBuilder.RenameIndex(
                name: "IX_OutputInfo_ProductId",
                table: "OutputInfo",
                newName: "IX_OutputInfo_ProductVarientId");

            migrationBuilder.RenameColumn(name: "CreatedByUserId", table: "Output", newName: "FinishedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Output_CreatedByUserId",
                table: "Output",
                newName: "IX_Output_FinishedBy");

            migrationBuilder.RenameColumn(name: "CreatedByUserId", table: "Input", newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Input_CreatedByUserId",
                table: "Input",
                newName: "IX_Input_CreatedBy");

            migrationBuilder.AddColumn<string>(
                name: "TaxIdentificationNumber",
                table: "Supplier",
                type: "varchar(20)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "Setting",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "ProductVariant",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "Product",
                type: "nvarchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "Product",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "Product",
                type: "nvarchar(255)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "OutputInfo",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Count",
                table: "OutputInfo",
                type: "int",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostPrice",
                table: "OutputInfo",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Output",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerAddress",
                table: "Output",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "Output",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastStatusChangedAt",
                table: "Output",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RemainingCount",
                table: "InputInfo",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "InputPrice",
                table: "InputInfo",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Count",
                table: "InputInfo",
                type: "int",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(name: "ParentOutputInfoId", table: "InputInfo", type: "int", nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfirmedBy",
                table: "Input",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(name: "SourceOrderId", table: "Input", type: "int", nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplierContact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    CitizenID = table.Column<string>(type: "varchar(20)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierContact_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(name: "IX_Output_CreatedBy", table: "Output", column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InputInfo_ParentOutputInfoId",
                table: "InputInfo",
                column: "ParentOutputInfoId");

            migrationBuilder.CreateIndex(name: "IX_Input_ConfirmedBy", table: "Input", column: "ConfirmedBy");

            migrationBuilder.CreateIndex(name: "IX_Input_SourceOrderId", table: "Input", column: "SourceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContact_SupplierId",
                table: "SupplierContact",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Input_Output_SourceOrderId",
                table: "Input",
                column: "SourceOrderId",
                principalTable: "Output",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Input_Users_ConfirmedBy",
                table: "Input",
                column: "ConfirmedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Input_Users_CreatedBy",
                table: "Input",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InputInfo_OutputInfo_ParentOutputInfoId",
                table: "InputInfo",
                column: "ParentOutputInfoId",
                principalTable: "OutputInfo",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Output_Users_CreatedBy",
                table: "Output",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Output_Users_FinishedBy",
                table: "Output",
                column: "FinishedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OutputInfo_ProductVariant_ProductVarientId",
                table: "OutputInfo",
                column: "ProductVarientId",
                principalTable: "ProductVariant",
                principalColumn: "Id");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Input_Output_SourceOrderId", table: "Input");

            migrationBuilder.DropForeignKey(name: "FK_Input_Users_ConfirmedBy", table: "Input");

            migrationBuilder.DropForeignKey(name: "FK_Input_Users_CreatedBy", table: "Input");

            migrationBuilder.DropForeignKey(name: "FK_InputInfo_OutputInfo_ParentOutputInfoId", table: "InputInfo");

            migrationBuilder.DropForeignKey(name: "FK_Output_Users_CreatedBy", table: "Output");

            migrationBuilder.DropForeignKey(name: "FK_Output_Users_FinishedBy", table: "Output");

            migrationBuilder.DropForeignKey(name: "FK_OutputInfo_ProductVariant_ProductVarientId", table: "OutputInfo");

            migrationBuilder.DropTable(name: "SupplierContact");

            migrationBuilder.DropIndex(name: "IX_Output_CreatedBy", table: "Output");

            migrationBuilder.DropIndex(name: "IX_InputInfo_ParentOutputInfoId", table: "InputInfo");

            migrationBuilder.DropIndex(name: "IX_Input_ConfirmedBy", table: "Input");

            migrationBuilder.DropIndex(name: "IX_Input_SourceOrderId", table: "Input");

            migrationBuilder.DropColumn(name: "TaxIdentificationNumber", table: "Supplier");

            migrationBuilder.DropColumn(name: "MetaDescription", table: "Product");

            migrationBuilder.DropColumn(name: "MetaTitle", table: "Product");

            migrationBuilder.DropColumn(name: "ShortDescription", table: "Product");

            migrationBuilder.DropColumn(name: "CreatedBy", table: "Output");

            migrationBuilder.DropColumn(name: "CustomerAddress", table: "Output");

            migrationBuilder.DropColumn(name: "CustomerPhone", table: "Output");

            migrationBuilder.DropColumn(name: "LastStatusChangedAt", table: "Output");

            migrationBuilder.DropColumn(name: "ParentOutputInfoId", table: "InputInfo");

            migrationBuilder.DropColumn(name: "ConfirmedBy", table: "Input");

            migrationBuilder.DropColumn(name: "SourceOrderId", table: "Input");

            migrationBuilder.RenameColumn(name: "ProductVarientId", table: "OutputInfo", newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OutputInfo_ProductVarientId",
                table: "OutputInfo",
                newName: "IX_OutputInfo_ProductId");

            migrationBuilder.RenameColumn(name: "FinishedBy", table: "Output", newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Output_FinishedBy",
                table: "Output",
                newName: "IX_Output_CreatedByUserId");

            migrationBuilder.RenameColumn(name: "CreatedBy", table: "Input", newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Input_CreatedBy",
                table: "Input",
                newName: "IX_Input_CreatedByUserId");

            migrationBuilder.AlterColumn<long>(
                name: "Value",
                table: "Setting",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Price",
                table: "ProductVariant",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Price",
                table: "OutputInfo",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "Count",
                table: "OutputInfo",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CostPrice",
                table: "OutputInfo",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(name: "EmpCode", table: "Output", type: "int", nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "RemainingCount",
                table: "InputInfo",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "InputPrice",
                table: "InputInfo",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "Count",
                table: "InputInfo",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Input_Users_CreatedByUserId",
                table: "Input",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Output_Users_CreatedByUserId",
                table: "Output",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OutputInfo_ProductVariant_ProductId",
                table: "OutputInfo",
                column: "ProductId",
                principalTable: "ProductVariant",
                principalColumn: "Id");
        }
    }
}
