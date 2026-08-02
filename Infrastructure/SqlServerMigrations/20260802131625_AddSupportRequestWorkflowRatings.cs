using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class AddSupportRequestWorkflowRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AssignedAt",
                table: "SupportRequest",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClosedAt",
                table: "SupportRequest",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CustomerRatedAt",
                table: "SupportRequest",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerRatingComment",
                table: "SupportRequest",
                type: "nvarchar(1000)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerRatingOfEmployee",
                table: "SupportRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerTrackingToken",
                table: "SupportRequest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmployeeRatedAt",
                table: "SupportRequest",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeRatingComment",
                table: "SupportRequest",
                type: "nvarchar(1000)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeRatingOfCustomer",
                table: "SupportRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartedAt",
                table: "SupportRequest",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "CustomerRatedAt",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "CustomerRatingComment",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "CustomerRatingOfEmployee",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "CustomerTrackingToken",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "EmployeeRatedAt",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "EmployeeRatingComment",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "EmployeeRatingOfCustomer",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "SupportRequest");
        }
    }
}
