using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class RemoveBaseEntityFromMappingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VariantOptionValue");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "VariantOptionValue");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VariantOptionValue");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProductCollectionPhoto");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductCollectionPhoto");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProductCollectionPhoto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedAt",
                table: "VariantOptionValue",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedAt",
                table: "VariantOptionValue",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAt",
                table: "VariantOptionValue",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAt",
                table: "ProductCollectionPhoto",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedAt",
                table: "ProductCollectionPhoto",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAt",
                table: "ProductCollectionPhoto",
                type: "bigint",
                nullable: true);
        }
    }
}
