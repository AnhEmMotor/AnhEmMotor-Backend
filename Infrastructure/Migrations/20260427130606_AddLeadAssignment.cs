using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToId",
                table: "Lead",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lead_AssignedToId",
                table: "Lead",
                column: "AssignedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lead_Users_AssignedToId",
                table: "Lead",
                column: "AssignedToId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lead_Users_AssignedToId",
                table: "Lead");

            migrationBuilder.DropIndex(
                name: "IX_Lead_AssignedToId",
                table: "Lead");

            migrationBuilder.DropColumn(
                name: "AssignedToId",
                table: "Lead");
        }
    }
}
