using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class AddPaymentLinkFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "PaymentCode", table: "Output", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<long>(name: "PaymentExpiredAt", table: "Output", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<string>(name: "PaymentUrl", table: "Output", type: "longtext", nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PaymentCode", table: "Output");
            migrationBuilder.DropColumn(name: "PaymentExpiredAt", table: "Output");
            migrationBuilder.DropColumn(name: "PaymentUrl", table: "Output");
        }
    }
}
