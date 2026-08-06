using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddChatAndProductViewAndStoreChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoutingContext",
                table: "ChatSession",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSteering",
                table: "ChatMessage",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "ReasoningElapsedSeconds",
                table: "ChatMessage",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasoningStepsJson",
                table: "ChatMessage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RunId",
                table: "ChatMessage",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatPlanTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanonicalQuestion = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    IntentHash = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    StepsTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slots = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredTools = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredPermissions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToolRegistryFingerprint = table.Column<string>(type: "nvarchar(32)", nullable: true),
                    Module = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    UseCount = table.Column<int>(type: "int", nullable: false),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    UserEditCount = table.Column<int>(type: "int", nullable: false),
                    RejectCount = table.Column<int>(type: "int", nullable: false),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatPlanTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatRun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    UserMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartialOutput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastSeq = table.Column<long>(type: "bigint", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    OwnerInstanceId = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    HeartbeatAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PendingSteering = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToolRegistryFingerprint = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    ModelUsed = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatRun_ChatSession_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChatSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductView",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CustomerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitorKey = table.Column<string>(type: "nvarchar(64)", nullable: true),
                    DwellTimeMs = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductView", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductView_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductView_Users_CustomerUserId",
                        column: x => x.CustomerUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StoreChatSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitorKey = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    CustomerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Mode = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    AssignedStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    PreviousSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreChatSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreChatSession_Users_CustomerUserId",
                        column: x => x.CustomerUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatFeedback",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatFeedback_ChatRun_ChatRunId",
                        column: x => x.ChatRunId,
                        principalTable: "ChatRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatFeedback_Users_ReportedBy",
                        column: x => x.ReportedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatPlan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Steps = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastEditedBy = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToolRegistryFingerprint = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                name: "ChatRunEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(40)", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRunEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatRunEvent_ChatRun_RunId",
                        column: x => x.RunId,
                        principalTable: "ChatRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreChatMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sender = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CardsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreChatMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreChatMessage_StoreChatSession_SessionId",
                        column: x => x.SessionId,
                        principalTable: "StoreChatSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_RunId",
                table: "ChatMessage",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatFeedback_ChatRunId",
                table: "ChatFeedback",
                column: "ChatRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatFeedback_ReportedBy",
                table: "ChatFeedback",
                column: "ReportedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ChatPlan_RunId",
                table: "ChatPlan",
                column: "RunId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRun_SessionId_Status",
                table: "ChatRun",
                columns: new[] { "SessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatRunEvent_RunId_Seq",
                table: "ChatRunEvent",
                columns: new[] { "RunId", "Seq" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductView_CustomerUserId_CreatedAt",
                table: "ProductView",
                columns: new[] { "CustomerUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductView_ProductId",
                table: "ProductView",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductView_VisitorKey_CreatedAt",
                table: "ProductView",
                columns: new[] { "VisitorKey", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StoreChatMessage_SessionId",
                table: "StoreChatMessage",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreChatSession_CustomerUserId",
                table: "StoreChatSession",
                column: "CustomerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreChatSession_Mode",
                table: "StoreChatSession",
                column: "Mode");

            migrationBuilder.CreateIndex(
                name: "IX_StoreChatSession_VisitorKey",
                table: "StoreChatSession",
                column: "VisitorKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessage_ChatRun_RunId",
                table: "ChatMessage",
                column: "RunId",
                principalTable: "ChatRun",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessage_ChatRun_RunId",
                table: "ChatMessage");

            migrationBuilder.DropTable(
                name: "ChatFeedback");

            migrationBuilder.DropTable(
                name: "ChatPlan");

            migrationBuilder.DropTable(
                name: "ChatPlanTemplate");

            migrationBuilder.DropTable(
                name: "ChatRunEvent");

            migrationBuilder.DropTable(
                name: "ProductView");

            migrationBuilder.DropTable(
                name: "StoreChatMessage");

            migrationBuilder.DropTable(
                name: "ChatRun");

            migrationBuilder.DropTable(
                name: "StoreChatSession");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessage_RunId",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "RoutingContext",
                table: "ChatSession");

            migrationBuilder.DropColumn(
                name: "IsSteering",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "ReasoningElapsedSeconds",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "ReasoningStepsJson",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "RunId",
                table: "ChatMessage");
        }
    }
}
