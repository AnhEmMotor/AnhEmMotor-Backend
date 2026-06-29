using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
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
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextMaintenanceDate",
                table: "MaintenanceHistory",
                type: "datetimeoffset",
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
                type: "nvarchar(MAX)",
                nullable: true);

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
