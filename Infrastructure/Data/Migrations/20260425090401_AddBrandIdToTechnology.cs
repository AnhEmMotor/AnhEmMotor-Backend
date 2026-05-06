using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandIdToTechnology : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "Technologies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Technologies_BrandId",
                table: "Technologies",
                column: "BrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Technologies_Brand_BrandId",
                table: "Technologies",
                column: "BrandId",
                principalTable: "Brand",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Technologies_Brand_BrandId",
                table: "Technologies");

            migrationBuilder.DropIndex(
                name: "IX_Technologies_BrandId",
                table: "Technologies");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Technologies");
        }
    }
}
