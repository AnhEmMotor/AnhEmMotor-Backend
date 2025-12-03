using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class EditSomeNullColumn : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_InputInfo_Input_InputId", table: "InputInfo");

            migrationBuilder.DropForeignKey(name: "FK_OutputInfo_Output_OutputId", table: "OutputInfo");

            migrationBuilder.AlterColumn<int>(
                name: "OutputId",
                table: "OutputInfo",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InputId",
                table: "InputInfo",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InputInfo_Input_InputId",
                table: "InputInfo",
                column: "InputId",
                principalTable: "Input",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OutputInfo_Output_OutputId",
                table: "OutputInfo",
                column: "OutputId",
                principalTable: "Output",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_InputInfo_Input_InputId", table: "InputInfo");

            migrationBuilder.DropForeignKey(name: "FK_OutputInfo_Output_OutputId", table: "OutputInfo");

            migrationBuilder.AlterColumn<int>(
                name: "OutputId",
                table: "OutputInfo",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "InputId",
                table: "InputInfo",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_InputInfo_Input_InputId",
                table: "InputInfo",
                column: "InputId",
                principalTable: "Input",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OutputInfo_Output_OutputId",
                table: "OutputInfo",
                column: "OutputId",
                principalTable: "Output",
                principalColumn: "id");
        }
    }
}
