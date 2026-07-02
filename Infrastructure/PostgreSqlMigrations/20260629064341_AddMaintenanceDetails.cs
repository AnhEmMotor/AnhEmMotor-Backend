using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
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
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceNumber",
                table: "MaintenanceHistory",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextMaintenanceDate",
                table: "MaintenanceHistory",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextMaintenanceOdo",
                table: "MaintenanceHistory",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PartsCost",
                table: "MaintenanceHistory",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PartsJson",
                table: "MaintenanceHistory",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "MaintenanceHistory",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "MaintenanceHistory",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceHistory_TechnicianId",
                table: "MaintenanceHistory",
                column: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory",
                column: "TechnicianId",
                principalTable: "EmployeeProfile",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceHistory_TechnicianId",
                table: "MaintenanceHistory");

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
