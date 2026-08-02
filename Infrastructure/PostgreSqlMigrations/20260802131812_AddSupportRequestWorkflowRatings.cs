using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
{
    /// <inheritdoc />
    public partial class AddSupportRequestWorkflowRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AssignedAt",
                table: "SupportRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClosedAt",
                table: "SupportRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CustomerRatedAt",
                table: "SupportRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerRatingComment",
                table: "SupportRequest",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerRatingOfEmployee",
                table: "SupportRequest",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerTrackingToken",
                table: "SupportRequest",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmployeeRatedAt",
                table: "SupportRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeRatingComment",
                table: "SupportRequest",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeRatingOfCustomer",
                table: "SupportRequest",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartedAt",
                table: "SupportRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NewsId",
                table: "NewsComments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ArticleSlug",
                table: "NewsComments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArticleType",
                table: "NewsComments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "MaintenanceHistory",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "DepositSettingHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrderThreshold = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DepositRatio = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositSettingHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_CreatedAt",
                table: "ChatMessage",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_SessionId",
                table: "ChatMessage",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSession_UpdatedAt",
                table: "ChatSession",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSession_UserId",
                table: "ChatSession",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessage");

            migrationBuilder.DropTable(
                name: "DepositSettingHistory");

            migrationBuilder.DropTable(
                name: "ChatSession");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "CustomerRatedAt",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "CustomerRatingComment",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "CustomerRatingOfEmployee",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "CustomerTrackingToken",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "EmployeeRatedAt",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "EmployeeRatingComment",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "EmployeeRatingOfCustomer",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "SupportRequest");

            migrationBuilder.DropColumn(
                name: "ArticleSlug",
                table: "NewsComments");

            migrationBuilder.DropColumn(
                name: "ArticleType",
                table: "NewsComments");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "MaintenanceHistory");

            migrationBuilder.AlterColumn<int>(
                name: "NewsId",
                table: "NewsComments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
