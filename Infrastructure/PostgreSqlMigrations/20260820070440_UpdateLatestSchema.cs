using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
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

            migrationBuilder.AddColumn<int>(
                name: "VariantColorId",
                table: "ProductView",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "ProductView",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedAt",
                table: "ProductView",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "VehicleImage",
                table: "Invoice",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "Invoice",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleVersion",
                table: "Invoice",
                type: "text",
                nullable: true);

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

            migrationBuilder.CreateTable(
                name: "ChatPlan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastEditedBy = table.Column<string>(type: "text", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Steps = table.Column<string>(type: "text", nullable: false),
                    ToolRegistryFingerprint = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CanonicalQuestion = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IntentHash = table.Column<string>(type: "text", nullable: false),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Module = table.Column<string>(type: "text", nullable: false),
                    RejectCount = table.Column<int>(type: "integer", nullable: false),
                    RequiredPermissions = table.Column<string>(type: "text", nullable: false),
                    RequiredTools = table.Column<string>(type: "text", nullable: false),
                    Slots = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StepsTemplate = table.Column<string>(type: "text", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    ToolRegistryFingerprint = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UseCount = table.Column<int>(type: "integer", nullable: false),
                    UserEditCount = table.Column<int>(type: "integer", nullable: false)
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
