using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaForProductViewInvoiceAndDropChatPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatPlan");

            migrationBuilder.DropTable(
                name: "ChatPlanTemplate");

            migrationBuilder.AddColumn<int>(
                name: "VariantColorId",
                table: "ProductView",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "ProductView",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedAt",
                table: "ProductView",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ProductView_VariantColorId",
                table: "ProductView",
                column: "VariantColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductView_VariantId",
                table: "ProductView",
                column: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductView_ProductVariantColor_VariantColorId",
                table: "ProductView",
                column: "VariantColorId",
                principalTable: "ProductVariantColor",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductView_ProductVariant_VariantId",
                table: "ProductView",
                column: "VariantId",
                principalTable: "ProductVariant",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductView_ProductVariantColor_VariantColorId",
                table: "ProductView");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductView_ProductVariant_VariantId",
                table: "ProductView");

            migrationBuilder.DropIndex(
                name: "IX_ProductView_VariantColorId",
                table: "ProductView");

            migrationBuilder.DropIndex(
                name: "IX_ProductView_VariantId",
                table: "ProductView");

            migrationBuilder.DropColumn(
                name: "VariantColorId",
                table: "ProductView");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "ProductView");

            migrationBuilder.DropColumn(
                name: "ViewedAt",
                table: "ProductView");

            migrationBuilder.CreateTable(
                name: "ChatPlan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastEditedBy = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Steps = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToolRegistryFingerprint = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatPlan_ChatRun_RunId",
                        column: x => x.RunId,
                        principalTable: "ChatRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatPlanTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanonicalQuestion = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IntentHash = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Module = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    RejectCount = table.Column<int>(type: "int", nullable: false),
                    RequiredPermissions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredTools = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slots = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    StepsTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    ToolRegistryFingerprint = table.Column<string>(type: "nvarchar(32)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UseCount = table.Column<int>(type: "int", nullable: false),
                    UserEditCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatPlanTemplate", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatPlan_RunId",
                table: "ChatPlan",
                column: "RunId",
                unique: true);
        }
    }
}
