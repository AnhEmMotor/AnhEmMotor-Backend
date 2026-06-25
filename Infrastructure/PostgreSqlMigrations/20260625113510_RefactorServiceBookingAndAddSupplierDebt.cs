using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
{
    /// <inheritdoc />
    public partial class RefactorServiceBookingAndAddSupplierDebt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ServiceBookings_Users_AssignedSaleId", table: "ServiceBookings");
            migrationBuilder.DropForeignKey(name: "FK_ServiceBookings_Vehicle_VehicleId", table: "ServiceBookings");
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceEvaluation_ServiceBookings_ServiceBookingId",
                table: "ServiceEvaluation");
            migrationBuilder.DropPrimaryKey(name: "PK_ServiceBookings", table: "ServiceBookings");
            migrationBuilder.DropColumn(name: "AdminNote", table: "ServiceBookings");
            migrationBuilder.DropColumn(name: "AppointmentTime", table: "ServiceBookings");
            migrationBuilder.DropColumn(name: "CancellationReason", table: "ServiceBookings");
            migrationBuilder.DropColumn(name: "CustomerNote", table: "ServiceBookings");
            migrationBuilder.RenameTable(name: "ServiceBookings", newName: "ServiceBooking");
            migrationBuilder.RenameColumn(name: "ServiceType", table: "ServiceBooking", newName: "PaymentStatus");
            migrationBuilder.RenameColumn(name: "CancelledAt", table: "ServiceBooking", newName: "CompletedDate");
            migrationBuilder.RenameColumn(name: "AssignedSaleId", table: "ServiceBooking", newName: "CustomerId");
            migrationBuilder.RenameColumn(name: "AppointmentDate", table: "ServiceBooking", newName: "ScheduledDate");
            migrationBuilder.RenameIndex(
                name: "IX_ServiceBookings_VehicleId",
                table: "ServiceBooking",
                newName: "IX_ServiceBooking_VehicleId");
            migrationBuilder.RenameIndex(
                name: "IX_ServiceBookings_AssignedSaleId",
                table: "ServiceBooking",
                newName: "IX_ServiceBooking_CustomerId");
            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "ServiceBooking",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ServiceBooking",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledDate",
                table: "ServiceBooking",
                type: "timestamp with time zone",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "CancelledReason",
                table: "ServiceBooking",
                type: "text",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "CustomerNotes",
                table: "ServiceBooking",
                type: "text",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "ServiceBooking",
                type: "numeric(18,2)",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "ServiceBooking",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<int>(name: "Rating", table: "ServiceBooking", type: "integer", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Review", table: "ServiceBooking", type: "text", nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "ServiceBooking",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "ServiceBooking",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "TechnicianNotes",
                table: "ServiceBooking",
                type: "text",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "ServiceBooking",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
            migrationBuilder.AddPrimaryKey(name: "PK_ServiceBooking", table: "ServiceBooking", column: "Id");
            migrationBuilder.CreateTable(
                name: "SupplierDebtSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EvidenceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDebtSettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDebtSettlements_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(
                name: "IX_ServiceBooking_ServiceId",
                table: "ServiceBooking",
                column: "ServiceId");
            migrationBuilder.CreateIndex(
                name: "IX_ServiceBooking_TechnicianId",
                table: "ServiceBooking",
                column: "TechnicianId");
            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebtSettlements_SupplierId",
                table: "SupplierDebtSettlements",
                column: "SupplierId");
            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBooking_EmployeeProfile_TechnicianId",
                table: "ServiceBooking",
                column: "TechnicianId",
                principalTable: "EmployeeProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBooking_Services_ServiceId",
                table: "ServiceBooking",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBooking_Users_CustomerId",
                table: "ServiceBooking",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBooking_Vehicle_VehicleId",
                table: "ServiceBooking",
                column: "VehicleId",
                principalTable: "Vehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_ServiceEvaluation_ServiceBooking_ServiceBookingId",
                table: "ServiceEvaluation",
                column: "ServiceBookingId",
                principalTable: "ServiceBooking",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceBooking_EmployeeProfile_TechnicianId",
                table: "ServiceBooking");
            migrationBuilder.DropForeignKey(name: "FK_ServiceBooking_Services_ServiceId", table: "ServiceBooking");
            migrationBuilder.DropForeignKey(name: "FK_ServiceBooking_Users_CustomerId", table: "ServiceBooking");
            migrationBuilder.DropForeignKey(name: "FK_ServiceBooking_Vehicle_VehicleId", table: "ServiceBooking");
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceEvaluation_ServiceBooking_ServiceBookingId",
                table: "ServiceEvaluation");
            migrationBuilder.DropTable(name: "SupplierDebtSettlements");
            migrationBuilder.DropPrimaryKey(name: "PK_ServiceBooking", table: "ServiceBooking");
            migrationBuilder.DropIndex(name: "IX_ServiceBooking_ServiceId", table: "ServiceBooking");
            migrationBuilder.DropIndex(name: "IX_ServiceBooking_TechnicianId", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "CancelledDate", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "CancelledReason", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "CustomerNotes", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "DepositAmount", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "EstimatedDurationMinutes", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "Rating", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "Review", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "ServiceId", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "TechnicianId", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "TechnicianNotes", table: "ServiceBooking");
            migrationBuilder.DropColumn(name: "TotalAmount", table: "ServiceBooking");
            migrationBuilder.RenameTable(name: "ServiceBooking", newName: "ServiceBookings");
            migrationBuilder.RenameColumn(name: "ScheduledDate", table: "ServiceBookings", newName: "AppointmentDate");
            migrationBuilder.RenameColumn(name: "PaymentStatus", table: "ServiceBookings", newName: "ServiceType");
            migrationBuilder.RenameColumn(name: "CustomerId", table: "ServiceBookings", newName: "AssignedSaleId");
            migrationBuilder.RenameColumn(name: "CompletedDate", table: "ServiceBookings", newName: "CancelledAt");
            migrationBuilder.RenameIndex(
                name: "IX_ServiceBooking_VehicleId",
                table: "ServiceBookings",
                newName: "IX_ServiceBookings_VehicleId");
            migrationBuilder.RenameIndex(
                name: "IX_ServiceBooking_CustomerId",
                table: "ServiceBookings",
                newName: "IX_ServiceBookings_AssignedSaleId");
            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "ServiceBookings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ServiceBookings",
                type: "text",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "ServiceBookings",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<TimeSpan>(
                name: "AppointmentTime",
                table: "ServiceBookings",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "ServiceBookings",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddColumn<string>(
                name: "CustomerNote",
                table: "ServiceBookings",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
            migrationBuilder.AddPrimaryKey(name: "PK_ServiceBookings", table: "ServiceBookings", column: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBookings_Users_AssignedSaleId",
                table: "ServiceBookings",
                column: "AssignedSaleId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
            migrationBuilder.AddForeignKey(
                name: "FK_ServiceBookings_Vehicle_VehicleId",
                table: "ServiceBookings",
                column: "VehicleId",
                principalTable: "Vehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_ServiceEvaluation_ServiceBookings_ServiceBookingId",
                table: "ServiceEvaluation",
                column: "ServiceBookingId",
                principalTable: "ServiceBookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
