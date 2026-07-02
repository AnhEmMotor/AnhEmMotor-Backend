using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class AddPlateDossierProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlateDossier_Output_OutputId",
                table: "PlateDossier");

            migrationBuilder.AlterColumn<int>(
                name: "OutputId",
                table: "PlateDossier",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<long>(
                name: "CompletedDate",
                table: "PlateDossier",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "PlateDossier",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "PlateDossier",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DossierNumber",
                table: "PlateDossier",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VinNumber",
                table: "PlateDossier",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.AddForeignKey(
                name: "FK_PlateDossier_Output_OutputId",
                table: "PlateDossier",
                column: "OutputId",
                principalTable: "Output",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_PlateDossier_Output_OutputId",
                table: "PlateDossier");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceHistory_TechnicianId",
                table: "MaintenanceHistory");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "PlateDossier");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "PlateDossier");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "PlateDossier");

            migrationBuilder.DropColumn(
                name: "DossierNumber",
                table: "PlateDossier");

            migrationBuilder.DropColumn(
                name: "VinNumber",
                table: "PlateDossier");

            migrationBuilder.AlterColumn<int>(
                name: "OutputId",
                table: "PlateDossier",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PlateDossier_Output_OutputId",
                table: "PlateDossier",
                column: "OutputId",
                principalTable: "Output",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
