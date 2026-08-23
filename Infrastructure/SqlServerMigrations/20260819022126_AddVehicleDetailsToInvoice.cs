using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class AddVehicleDetailsToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Shipments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "VehicleImage",
                table: "Invoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "Invoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleVersion",
                table: "Invoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_DeletedAt_Status_DeliveredAt_Type_OutputId",
                table: "Shipments",
                columns: new[] { "DeletedAt", "Status", "DeliveredAt", "Type", "OutputId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_DeletedAt_Status_DeliveredAt_Type_OutputId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "VehicleImage",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "VehicleVersion",
                table: "Invoice");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
