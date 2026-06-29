using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class AddIsVerifiedToLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Lead",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Lead");
        }
    }
}
