using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class AddChatBotAndExtendSupportRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: AssignedAt, ClosedAt, CustomerRatedAt, CustomerRatingComment,
            // CustomerRatingOfEmployee, CustomerTrackingToken, EmployeeRatedAt,
            // EmployeeRatingComment, EmployeeRatingOfCustomer, StartedAt
            // were already added by migration 20260802131625_AddSupportRequestWorkflowRatings
            // and exist in the DB — skipped here to avoid duplicate column error.
            migrationBuilder.AlterColumn<int>(
                name: "NewsId",
                table: "NewsComments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
            // NOTE: Tables ChatPlanTemplate, ChatSession, etc. already exist in the database.
            // All CreateTable and AddColumn commands have been commented out to avoid migration errors.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ChatFeedback");
            migrationBuilder.DropTable(name: "ChatMessage");
            migrationBuilder.DropTable(name: "ChatPlan");
            migrationBuilder.DropTable(name: "ChatPlanTemplate");
            migrationBuilder.DropTable(name: "ChatRunEvent");
            migrationBuilder.DropTable(name: "DepositSettingHistory");
            migrationBuilder.DropTable(name: "ProductView");
            migrationBuilder.DropTable(name: "StoreChatMessage");
            migrationBuilder.DropTable(name: "ChatRun");
            migrationBuilder.DropTable(name: "StoreChatSession");
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
