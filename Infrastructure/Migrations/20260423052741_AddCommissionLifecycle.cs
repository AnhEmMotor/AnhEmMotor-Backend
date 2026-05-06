using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissionLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "CommissionRecord",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "PolicySnapshot",
                table: "CommissionRecord",
                type: "nvarchar(MAX)",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CommissionRecord",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.CreateTable(
                name: "CommissionPolicyAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    PolicyId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    ChangedByName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldValueSnapshot = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    NewValueSnapshot = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionPolicyAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionPolicyAuditLog_CommissionPolicy_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "CommissionPolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(
                name: "IX_CommissionPolicyAuditLog_PolicyId",
                table: "CommissionPolicyAuditLog",
                column: "PolicyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CommissionPolicyAuditLog");
            migrationBuilder.DropColumn(name: "PaidAt", table: "CommissionRecord");
            migrationBuilder.DropColumn(name: "PolicySnapshot", table: "CommissionRecord");
            migrationBuilder.DropColumn(name: "Status", table: "CommissionRecord");
        }
    }
}
