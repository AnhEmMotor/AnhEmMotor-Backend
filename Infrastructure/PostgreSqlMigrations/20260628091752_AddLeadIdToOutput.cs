using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
{
    /// <inheritdoc />
    public partial class AddLeadIdToOutput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LeadId",
                table: "Output",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Output_LeadId",
                table: "Output",
                column: "LeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Output_Lead_LeadId",
                table: "Output",
                column: "LeadId",
                principalTable: "Lead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Output_Lead_LeadId",
                table: "Output");

            migrationBuilder.DropIndex(
                name: "IX_Output_LeadId",
                table: "Output");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "Output");
        }
    }
}
