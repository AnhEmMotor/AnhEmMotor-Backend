using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSomeColumnInInputAndOutput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Output_Users_CompletedByUserId",
                table: "Output");

            migrationBuilder.RenameColumn(
                name: "CompletedByUserId",
                table: "Output",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Output_CompletedByUserId",
                table: "Output",
                newName: "IX_Output_CreatedByUserId");

            migrationBuilder.AddColumn<int>(
                name: "EmpCode",
                table: "Output",
                type: "int",
                nullable: true);

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
                name: "FK_Output_Users_CreatedByUserId",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "EmpCode",
                table: "Output");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Output",
                newName: "CompletedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Output_CreatedByUserId",
                table: "Output",
                newName: "IX_Output_CompletedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Output_Users_CompletedByUserId",
                table: "Output",
                column: "CompletedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
