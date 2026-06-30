using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
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
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedDate",
                table: "PlateDossier",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "PlateDossier",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "PlateDossier",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DossierNumber",
                table: "PlateDossier",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VinNumber",
                table: "PlateDossier",
                type: "text",
                nullable: false,
                defaultValue: "");

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
                name: "FK_PlateDossier_Output_OutputId",
                table: "PlateDossier");

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
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
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
