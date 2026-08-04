using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    /// <inheritdoc />
    public partial class AddSupportRequestWorkflowRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AssignedAt",
                table: "SupportRequest",
                type: "bigint",
                nullable: true);
            migrationBuilder.AddColumn<long>(name: "ClosedAt", table: "SupportRequest", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<long>(
                name: "CustomerRatedAt",
                table: "SupportRequest",
                type: "bigint",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "CustomerRatingComment",
                table: "SupportRequest",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<int>(
                name: "CustomerRatingOfEmployee",
                table: "SupportRequest",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerTrackingToken",
                table: "SupportRequest",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");
            migrationBuilder.AddColumn<long>(
                name: "EmployeeRatedAt",
                table: "SupportRequest",
                type: "bigint",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "EmployeeRatingComment",
                table: "SupportRequest",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<int>(
                name: "EmployeeRatingOfCustomer",
                table: "SupportRequest",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<long>(name: "StartedAt", table: "SupportRequest", type: "bigint", nullable: true);
            migrationBuilder.AlterColumn<int>(
                name: "NewsId",
                table: "NewsComments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.AddColumn<string>(
                name: "ArticleSlug",
                table: "NewsComments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "ArticleType",
                table: "NewsComments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "MaintenanceHistory",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "ChatSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatSession_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "DepositSettingHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrderType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrderThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositRatio = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositSettingHistory", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateTable(
                name: "ChatMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SessionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessage_ChatSession_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChatSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
            migrationBuilder.CreateIndex(name: "IX_ChatMessage_CreatedAt", table: "ChatMessage", column: "CreatedAt");
            migrationBuilder.CreateIndex(name: "IX_ChatMessage_SessionId", table: "ChatMessage", column: "SessionId");
            migrationBuilder.CreateIndex(name: "IX_ChatSession_UpdatedAt", table: "ChatSession", column: "UpdatedAt");
            migrationBuilder.CreateIndex(name: "IX_ChatSession_UserId", table: "ChatSession", column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ChatMessage");
            migrationBuilder.DropTable(name: "DepositSettingHistory");
            migrationBuilder.DropTable(name: "ChatSession");
            migrationBuilder.DropColumn(name: "AssignedAt", table: "SupportRequest");
            migrationBuilder.DropColumn(name: "ClosedAt", table: "SupportRequest");
            migrationBuilder.DropColumn(name: "CustomerRatedAt", table: "SupportRequest");
            migrationBuilder.DropColumn(name: "CustomerRatingComment", table: "SupportRequest");
            migrationBuilder.DropColumn(name: "CustomerRatingOfEmployee", table: "SupportRequest");
            migrationBuilder.DropColumn(name: "CustomerTrackingToken", table: "SupportRequest");
            migrationBuilder.DropColumn(name: "EmployeeRatedAt", table: "SupportRequest");
            migrationBuilder.DropColumn(name: "EmployeeRatingComment", table: "SupportRequest");
            migrationBuilder.DropColumn(name: "EmployeeRatingOfCustomer", table: "SupportRequest");
            migrationBuilder.DropColumn(name: "StartedAt", table: "SupportRequest");
            migrationBuilder.DropColumn(name: "ArticleSlug", table: "NewsComments");
            migrationBuilder.DropColumn(name: "ArticleType", table: "NewsComments");
            migrationBuilder.DropColumn(name: "ServiceType", table: "MaintenanceHistory");
            migrationBuilder.AlterColumn<int>(
                name: "NewsId",
                table: "NewsComments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
