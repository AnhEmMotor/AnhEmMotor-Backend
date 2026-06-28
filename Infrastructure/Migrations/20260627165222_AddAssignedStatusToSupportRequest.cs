using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedStatusToSupportRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SupportRequest_AssignedUserId",
                table: "SupportRequest",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportRequest_Users_AssignedUserId",
                table: "SupportRequest",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportRequest_Users_AssignedUserId",
                table: "SupportRequest");

            migrationBuilder.DropIndex(
                name: "IX_SupportRequest_AssignedUserId",
                table: "SupportRequest");
        }
    }
}
