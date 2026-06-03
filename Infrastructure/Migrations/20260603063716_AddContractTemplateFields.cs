using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContractTemplateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractTemplates_ContractTemplates_ParentId",
                table: "ContractTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ContractTemplates_ParentId",
                table: "ContractTemplates");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ContractTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Version",
                table: "ContractTemplates",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "ContractTemplates");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ContractTemplates");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplates_ParentId",
                table: "ContractTemplates",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractTemplates_ContractTemplates_ParentId",
                table: "ContractTemplates",
                column: "ParentId",
                principalTable: "ContractTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
