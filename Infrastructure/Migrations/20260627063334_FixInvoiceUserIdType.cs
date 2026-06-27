using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixInvoiceUserIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoice_UserId1",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Users_UserId1",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Invoice");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Invoice",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_UserId",
                table: "Invoice",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Users_UserId",
                table: "Invoice",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Users_UserId",
                table: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_UserId",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Invoice");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Invoice",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Invoice",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_UserId1",
                table: "Invoice",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Users_UserId1",
                table: "Invoice",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
