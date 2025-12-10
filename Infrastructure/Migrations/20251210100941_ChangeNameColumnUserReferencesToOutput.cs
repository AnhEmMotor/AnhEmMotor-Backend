using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameColumnUserReferencesToOutput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Input_Users_FinishedByUserId",
                table: "Input");

            migrationBuilder.DropForeignKey(
                name: "FK_Output_Users_EmployeeCompletedId",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "EmpCode",
                table: "Output");

            migrationBuilder.RenameColumn(
                name: "EmployeeCompletedId",
                table: "Output",
                newName: "CompletedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Output_EmployeeCompletedId",
                table: "Output",
                newName: "IX_Output_CompletedByUserId");

            migrationBuilder.RenameColumn(
                name: "FinishedByUserId",
                table: "Input",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Input_FinishedByUserId",
                table: "Input",
                newName: "IX_Input_CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Input_Users_CreatedByUserId",
                table: "Input",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Output_Users_CompletedByUserId",
                table: "Output",
                column: "CompletedByUserId",
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
                name: "FK_Output_Users_CompletedByUserId",
                table: "Output");

            migrationBuilder.RenameColumn(
                name: "CompletedByUserId",
                table: "Output",
                newName: "EmployeeCompletedId");

            migrationBuilder.RenameIndex(
                name: "IX_Output_CompletedByUserId",
                table: "Output",
                newName: "IX_Output_EmployeeCompletedId");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Input",
                newName: "FinishedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Input_CreatedByUserId",
                table: "Input",
                newName: "IX_Input_FinishedByUserId");

            migrationBuilder.AddColumn<int>(
                name: "EmpCode",
                table: "Output",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Input_Users_FinishedByUserId",
                table: "Input",
                column: "FinishedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Output_Users_EmployeeCompletedId",
                table: "Output",
                column: "EmployeeCompletedId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
