using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionNameFkToPredefinedOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PredefinedOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredefinedOption", x => x.Id);
                    table.UniqueConstraint("AK_PredefinedOption_Key", x => x.Key);
                });
            migrationBuilder.CreateIndex(name: "IX_Option_Name", table: "Option", column: "Name");
            migrationBuilder.CreateIndex(
                name: "IX_PredefinedOption_Key",
                table: "PredefinedOption",
                column: "Key",
                unique: true);
            migrationBuilder.AddForeignKey(
                name: "FK_Option_PredefinedOption_Name",
                table: "Option",
                column: "Name",
                principalTable: "PredefinedOption",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Option_PredefinedOption_Name", table: "Option");
            migrationBuilder.DropTable(name: "PredefinedOption");
            migrationBuilder.DropIndex(name: "IX_Option_Name", table: "Option");
        }
    }
}
