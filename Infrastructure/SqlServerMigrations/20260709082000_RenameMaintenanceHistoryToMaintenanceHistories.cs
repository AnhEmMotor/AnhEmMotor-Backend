using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqlServerMigrations
{
    public partial class RenameMaintenanceHistoryToMaintenanceHistories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "MaintenanceHistory",
                newName: "MaintenanceHistories");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceHistory_TechnicianId",
                table: "MaintenanceHistories",
                newName: "IX_MaintenanceHistories_TechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceHistory_VehicleId",
                table: "MaintenanceHistories",
                newName: "IX_MaintenanceHistories_VehicleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "MaintenanceHistories",
                newName: "MaintenanceHistory");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceHistories_TechnicianId",
                table: "MaintenanceHistory",
                newName: "IX_MaintenanceHistory_TechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceHistories_VehicleId",
                table: "MaintenanceHistory",
                newName: "IX_MaintenanceHistory_VehicleId");
        }
    }
}
