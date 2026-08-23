using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class UpdateLatestSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatPlan");

            migrationBuilder.DropTable(
                name: "ChatPlanTemplate");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Shipments",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

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
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "VehicleImage",
                table: "Invoice",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "Invoice",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VehicleVersion",
                table: "Invoice",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_DeletedAt_Status_DeliveredAt_Type_OutputId",
                table: "Shipments",
                columns: new[] { "DeletedAt", "Status", "DeliveredAt", "Type", "OutputId" });

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
                name: "IX_Shipments_DeletedAt_Status_DeliveredAt_Type_OutputId",
                table: "Shipments");

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

            migrationBuilder.DropColumn(
                name: "VehicleImage",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "VehicleVersion",
                table: "Invoice");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Shipments",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChatPlan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RunId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApprovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true),
                    LastEditedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SessionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Steps = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToolRegistryFingerprint = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChatPlanTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CanonicalQuestion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true),
                    IntentHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastUsedAt = table.Column<long>(type: "bigint", nullable: true),
                    Module = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RejectCount = table.Column<int>(type: "int", nullable: false),
                    RequiredPermissions = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiredTools = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slots = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StepsTemplate = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    ToolRegistryFingerprint = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UseCount = table.Column<int>(type: "int", nullable: false),
                    UserEditCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatPlanTemplate", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChatPlan_RunId",
                table: "ChatPlan",
                column: "RunId",
                unique: true);
        }
    }
}
