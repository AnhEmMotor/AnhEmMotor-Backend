using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserReferencesToInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BuyerId",
                table: "Output",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Output",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Input",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Output_BuyerId",
                table: "Output",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Output_CreatedByUserId",
                table: "Output",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Input_CreatedByUserId",
                table: "Input",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Input_Users_CreatedByUserId",
                table: "Input",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Output_Users_BuyerId",
                table: "Output",
                column: "BuyerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Output_Users_CreatedByUserId",
                table: "Output",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Input_Users_CreatedByUserId",
                table: "Input");

            migrationBuilder.DropForeignKey(
                name: "FK_Output_Users_BuyerId",
                table: "Output");

            migrationBuilder.DropForeignKey(
                name: "FK_Output_Users_CreatedByUserId",
                table: "Output");

            migrationBuilder.DropIndex(
                name: "IX_Output_BuyerId",
                table: "Output");

            migrationBuilder.DropIndex(
                name: "IX_Output_CreatedByUserId",
                table: "Output");

            migrationBuilder.DropIndex(
                name: "IX_Input_CreatedByUserId",
                table: "Input");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Input");
        }
    }
}
