using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class AddCompanyInvoiceToOutput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BudgetCode",
                table: "Output",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyAddress",
                table: "Output",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyEmail",
                table: "Output",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Output",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyTaxCode",
                table: "Output",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompanyInvoice",
                table: "Output",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BudgetCode",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "CompanyAddress",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "CompanyEmail",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "CompanyTaxCode",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "IsCompanyInvoice",
                table: "Output");
        }
    }
}
