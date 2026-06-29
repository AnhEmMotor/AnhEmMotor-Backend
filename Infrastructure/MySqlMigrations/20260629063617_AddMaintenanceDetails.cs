using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LaborCost",
                table: "MaintenanceHistory",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceNumber",
                table: "MaintenanceHistory",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "NextMaintenanceDate",
                table: "MaintenanceHistory",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextMaintenanceOdo",
                table: "MaintenanceHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PartsCost",
                table: "MaintenanceHistory",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PartsJson",
                table: "MaintenanceHistory",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "MaintenanceHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "MaintenanceHistory",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaborCost",
                table: "MaintenanceHistory");

            migrationBuilder.DropColumn(
                name: "MaintenanceNumber",
                table: "MaintenanceHistory");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceDate",
                table: "MaintenanceHistory");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceOdo",
                table: "MaintenanceHistory");

            migrationBuilder.DropColumn(
                name: "PartsCost",
                table: "MaintenanceHistory");

            migrationBuilder.DropColumn(
                name: "PartsJson",
                table: "MaintenanceHistory");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "MaintenanceHistory");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "MaintenanceHistory");
        }
    }
}
