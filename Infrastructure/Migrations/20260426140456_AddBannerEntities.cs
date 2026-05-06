using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Position",
                table: "Banner",
                newName: "Placement");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Banner",
                newName: "MobileImageUrl");

            migrationBuilder.RenameColumn(
                name: "DisplayOrder",
                table: "Banner",
                newName: "ViewCount");

            migrationBuilder.AddColumn<int>(
                name: "ClickCount",
                table: "Banner",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CtaText",
                table: "Banner",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesktopImageUrl",
                table: "Banner",
                type: "nvarchar(500)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Banner",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BannerAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BannerId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BannerAuditLog_Banner_BannerId",
                        column: x => x.BannerId,
                        principalTable: "Banner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannerAuditLog_BannerId",
                table: "BannerAuditLog",
                column: "BannerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannerAuditLog");

            migrationBuilder.DropColumn(
                name: "ClickCount",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "CtaText",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "DesktopImageUrl",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Banner");

            migrationBuilder.RenameColumn(
                name: "ViewCount",
                table: "Banner",
                newName: "DisplayOrder");

            migrationBuilder.RenameColumn(
                name: "Placement",
                table: "Banner",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "MobileImageUrl",
                table: "Banner",
                newName: "ImageUrl");
        }
    }
}
