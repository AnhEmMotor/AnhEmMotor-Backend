using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class FixProductCategoryCharset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "ProductCategory")
                .Annotation("MySQL:Charset", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci")
                .OldAnnotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "ProductCategory")
                .Annotation("MySQL:Charset", "utf8mb4")
                .OldAnnotation("MySQL:Charset", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");
        }
    }
}
