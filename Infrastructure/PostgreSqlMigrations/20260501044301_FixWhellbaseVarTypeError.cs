using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
{
    /// <inheritdoc />
    public partial class FixWhellbaseVarTypeError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Wheelbase",
                table: "Product",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Wheelbase",
                table: "Product",
                type: "text",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }
    }
}
