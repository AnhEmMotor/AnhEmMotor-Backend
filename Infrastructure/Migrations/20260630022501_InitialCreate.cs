using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    DesktopImageUrl = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    MobileImageUrl = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    CtaLink = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    CtaLabel = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Placement = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banner", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Brand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CarrierPartners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarrierCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebhookSecret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebhookEndpointUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AutoSyncPricing = table.Column<bool>(type: "bit", nullable: false),
                    MaxParcelWeightKg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AllowLiquidCargo = table.Column<bool>(type: "bit", nullable: false),
                    AllowOversizeCargo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarrierPartners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    InternalNote = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrentUnreconciledCods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentUnreconciledCods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinanceContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoanAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DisbursementStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CavetLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryReceiptStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptStatus", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StorageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileExtension = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Slug = table.Column<string>(type: "varchar(255)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderLogistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    BottleneckDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBottleneck = table.Column<bool>(type: "bit", nullable: false),
                    DriverName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DriverPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentLat = table.Column<double>(type: "float", nullable: false),
                    CurrentLng = table.Column<double>(type: "float", nullable: false),
                    EstimatedArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLogistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutputStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutputStatus", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "ParcelDeliveryOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Carrier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CodAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoxCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnProofImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnInternalNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalOrderCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelDeliveryOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartnerType",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerType", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PredefinedOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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

            migrationBuilder.CreateTable(
                name: "ProductCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    MaxPurchaseQuantity = table.Column<int>(type: "int", nullable: true),
                    ManagementType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategory_ProductCategory_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ProductCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStatus", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "SupplierStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierStatus", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "TechnologyCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnologyCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "CustomerFeedback",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    FeedbackArea = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerFeedback_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobApplication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    AppliedPosition = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    CvFileUrl = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    CoverLetter = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplication_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParcelDeliveryOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParcelDeliveryOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShelfLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsPicked = table.Column<bool>(type: "bit", nullable: false),
                    IsRestricted = table.Column<bool>(type: "bit", nullable: false),
                    IsOutOfStock = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelDeliveryOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelDeliveryOrderItems_ParcelDeliveryOrders_ParcelDeliveryOrderId",
                        column: x => x.ParcelDeliveryOrderId,
                        principalTable: "ParcelDeliveryOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Option",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Option", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Option_PredefinedOption_Name",
                        column: x => x.Name,
                        principalTable: "PredefinedOption",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    ShortDescription = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BrandId = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    Dimensions = table.Column<string>(type: "nvarchar(35)", nullable: true),
                    Wheelbase = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    SeatHeight = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    GroundClearance = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    FuelCapacity = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    TireSize = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    FrontSuspension = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    RearSuspension = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    EngineType = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    MaxPower = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    OilCapacity = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    FuelConsumption = table.Column<string>(type: "nvarchar(35)", nullable: true),
                    TransmissionType = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    StarterSystem = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    MaxTorque = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    Displacement = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    BoreStroke = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    CompressionRatio = table.Column<string>(type: "nvarchar(10)", nullable: true),
                    FuelSystem = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    FrameType = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    FrontTireSize = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    RearTireSize = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    FrontBrake = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    RearBrake = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    BatteryType = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    LightingSystem = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    DashboardType = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Material = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    WarrantyPeriod = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    StdDot = table.Column<bool>(type: "bit", nullable: false),
                    StdEce = table.Column<bool>(type: "bit", nullable: false),
                    StdSnell = table.Column<bool>(type: "bit", nullable: false),
                    StdJis = table.Column<bool>(type: "bit", nullable: false),
                    OtherStandards = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    MaxPurchaseQuantity = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brand",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Product_ProductCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Product_ProductStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ProductStatus",
                        principalColumn: "Key");
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_ServiceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    StatusId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    TaxIdentificationNumber = table.Column<string>(type: "varchar(20)", nullable: true),
                    PartnerTypeId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Supplier_PartnerType_PartnerTypeId",
                        column: x => x.PartnerTypeId,
                        principalTable: "PartnerType",
                        principalColumn: "Key");
                    table.ForeignKey(
                        name: "FK_Supplier_SupplierStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "SupplierStatus",
                        principalColumn: "Key");
                });

            migrationBuilder.CreateTable(
                name: "Technologies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    BrandId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    DefaultTitle = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    DefaultDescription = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    DefaultImageUrl = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technologies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Technologies_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brand",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Technologies_TechnologyCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TechnologyCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ContactReply",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    RepliedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactReply", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactReply_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactReply_Users_RepliedById",
                        column: x => x.RepliedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerContact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ProcessedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RepliedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    InternalNote = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerContact_Users_ProcessedBy",
                        column: x => x.ProcessedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeProfile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    ContractDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeProfile_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerIdCard = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleModel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChassisNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EngineNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehiclePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RegistrationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InsuranceFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SalesPerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoice_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lead",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    InterestedVehicle = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    AddressDetail = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    Ward = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Province = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdentificationNumber = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lead_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Slug = table.Column<string>(type: "varchar(255)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    PublishedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    MetaTitle = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                    table.ForeignKey(
                        name: "FK_News_NewsCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "NewsCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_News_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NewsArticle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Excerpt = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    SeoTitle = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    SeoDescription = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    SeoKeywords = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PublishedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsArticle_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NewsArticle_Users_PublishedBy",
                        column: x => x.PublishedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PromotionBanner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionBanner", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionBanner_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PromotionBanner_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "varchar(30)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SentBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequest_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseRequest_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseRequest_Users_RejectedBy",
                        column: x => x.RejectedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseRequest_Users_SentBy",
                        column: x => x.SentBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupportRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    OrderCode = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportRequest_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportRequest_Users_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SupportTicket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SLADeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTicket_Users_AssignedAdminId",
                        column: x => x.AssignedAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupportTicket_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptionValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OptionId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoTitle = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    SeoDescription = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionValue_Option_OptionId",
                        column: x => x.OptionId,
                        principalTable: "Option",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CommissionPolicy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TargetGroup = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    EffectiveDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionPolicy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionPolicy_ProductCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommissionPolicy_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductCompatibility",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseProductId = table.Column<int>(type: "int", nullable: false),
                    CompatibleVehicleModelId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCompatibility", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCompatibility_Product_BaseProductId",
                        column: x => x.BaseProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCompatibility_Product_CompatibleVehicleModelId",
                        column: x => x.CompatibleVehicleModelId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UrlSlug = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    VariantName = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    SKU = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Dimensions = table.Column<string>(type: "nvarchar(35)", nullable: true),
                    Wheelbase = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SeatHeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GroundClearance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FuelCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TireSize = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    FrontBrake = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    RearBrake = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    FrontSuspension = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    RearSuspension = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    EngineType = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    MaxPurchaseQuantity = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariant_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierContact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    CitizenID = table.Column<string>(type: "varchar(20)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierContact_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    ContractNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContractFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContractValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Terms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentWindowDays = table.Column<int>(type: "int", nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MinimumVolumePerMonth = table.Column<int>(type: "int", nullable: true),
                    DiscountRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ParentContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierContracts_SupplierContracts_ParentContractId",
                        column: x => x.ParentContractId,
                        principalTable: "SupplierContracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierContracts_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierDebtLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingDebt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDebtLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDebtLog_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierDebtLog_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierDebtSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDebtSettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDebtSettlements_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierFinances",
                columns: table => new
                {
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    CurrentDebt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierFinances", x => x.SupplierId);
                    table.ForeignKey(
                        name: "FK_SupplierFinances_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTechnology",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TechnologyId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CustomTitle = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    CustomDescription = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CustomImageUrl = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTechnology", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTechnology_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTechnology_Technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "Technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnologyImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TechnologyId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnologyImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnologyImages_Technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "Technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileId = table.Column<int>(type: "int", nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPI_EmployeeProfile_EmployeeProfileId",
                        column: x => x.EmployeeProfileId,
                        principalTable: "EmployeeProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payroll",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCommission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Bonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Penalty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payroll", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payroll_EmployeeProfile_EmployeeProfileId",
                        column: x => x.EmployeeProfileId,
                        principalTable: "EmployeeProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadActivity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadId = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadActivity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadActivity_Lead_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Output",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastStatusChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BuyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FinishedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StatusId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    DepositRatio = table.Column<int>(type: "int", nullable: true),
                    PaymentUrl = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    PaymentCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentExpiredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Output", x => x.id);
                    table.ForeignKey(
                        name: "FK_Output_Lead_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Output_OutputStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "OutputStatus",
                        principalColumn: "Key");
                    table.ForeignKey(
                        name: "FK_Output_Users_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Output_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Output_Users_FinishedBy",
                        column: x => x.FinishedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NewsComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    AuthorEmail = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsComments_News_NewsId",
                        column: x => x.NewsId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequestAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseRequestId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OldStatusId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewStatusId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequestAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestAuditLog_PurchaseRequest_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestAuditLog_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerContactReply",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    ReplyContent = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false),
                    RepliedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SupportTicketId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerContactReply", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerContactReply_CustomerContact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "CustomerContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerContactReply_SupportTicket_SupportTicketId",
                        column: x => x.SupportTicketId,
                        principalTable: "SupportTicket",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomerContactReply_Users_RepliedBy",
                        column: x => x.RepliedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CommissionPolicyAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    PreferredDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    BookingType = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Booking_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BookingAppointment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    PreferredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreferredTimeSlot = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    AppointmentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Showroom = table.Column<string>(type: "nvarchar(150)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ConfirmedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingAppointment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingAppointment_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BookingAppointment_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StockBefore = table.Column<int>(type: "int", nullable: false),
                    StockAfter = table.Column<int>(type: "int", nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    PerformedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PerformedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransaction_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransaction_Users_PerformedBy",
                        column: x => x.PerformedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProductCollectionPhoto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCollectionPhoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCollectionPhoto_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariantColor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    MaxPurchaseQuantity = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariantColor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariantColor_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VariantOptionValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariantId = table.Column<int>(type: "int", nullable: false),
                    OptionValueId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantOptionValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VariantOptionValue_OptionValue_OptionValueId",
                        column: x => x.OptionValueId,
                        principalTable: "OptionValue",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VariantOptionValue_ProductVariant_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierContractAuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierContractAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierContractAuditLog_SupplierContracts_SupplierContractId",
                        column: x => x.SupplierContractId,
                        principalTable: "SupplierContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierContractItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    WholesalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierContractItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierContractItem_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierContractItem_SupplierContracts_SupplierContractId",
                        column: x => x.SupplierContractId,
                        principalTable: "SupplierContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierDebtLogImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierDebtLogId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDebtLogImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDebtLogImages_SupplierDebtLog_SupplierDebtLogId",
                        column: x => x.SupplierDebtLogId,
                        principalTable: "SupplierDebtLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommissionRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileId = table.Column<int>(type: "int", nullable: false),
                    OutputId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateEarned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PolicySnapshot = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionRecord_EmployeeProfile_EmployeeProfileId",
                        column: x => x.EmployeeProfileId,
                        principalTable: "EmployeeProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommissionRecord_Output_OutputId",
                        column: x => x.OutputId,
                        principalTable: "Output",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryReceipt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryReceiptDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    StatusId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PurchaseRequestId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfirmedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SentBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceOrderId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_InventoryReceiptStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "InventoryReceiptStatus",
                        principalColumn: "Key");
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_Output_SourceOrderId",
                        column: x => x.SourceOrderId,
                        principalTable: "Output",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_PurchaseRequest_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_Users_RejectedBy",
                        column: x => x.RejectedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryReceipt_Users_SentBy",
                        column: x => x.SentBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OutputId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Output_OutputId",
                        column: x => x.OutputId,
                        principalTable: "Output",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PlateDossier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OutputId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    RegistrationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ServiceFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    DossierNumber = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    VinNumber = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    CompletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateDossier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlateDossier_Output_OutputId",
                        column: x => x.OutputId,
                        principalTable: "Output",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "SalesContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OutputId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShowroomName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowroomTaxCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowroomAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowroomRepresentative = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerCCCD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FrameNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualSalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalPaymentDeadline = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    WarrantyPeriod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarrantyScope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecialTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ScannedFileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesContracts_Output_OutputId",
                        column: x => x.OutputId,
                        principalTable: "Output",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_SalesContracts_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InventoryLedger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DocumentCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    PartnerName = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    ImportQty = table.Column<int>(type: "int", nullable: false),
                    ExportQty = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockAfter = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLedger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryLedger_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryLedger_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryOnHand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    BeginningQty = table.Column<int>(type: "int", nullable: false),
                    StockQty = table.Column<int>(type: "int", nullable: false),
                    ImportedQty = table.Column<int>(type: "int", nullable: false),
                    ExportedQty = table.Column<int>(type: "int", nullable: false),
                    OrderedQty = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryOnHand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryOnHand_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryOnHand_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NewsProduct",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsProduct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsProduct_News_NewsId",
                        column: x => x.NewsId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsProduct_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NewsProduct_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutputInfo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    Count = table.Column<int>(type: "int", nullable: true),
                    OutputId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutputInfo", x => x.id);
                    table.ForeignKey(
                        name: "FK_OutputInfo_Output_OutputId",
                        column: x => x.OutputId,
                        principalTable: "Output",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OutputInfo_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OutputInfo_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductQuotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    QuotePrice = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductQuotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductQuotations_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductQuotations_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductQuotations_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InventoryReceiptAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryReceiptId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OldStatusId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewStatusId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptAuditLog_InventoryReceipt_InventoryReceiptId",
                        column: x => x.InventoryReceiptId,
                        principalTable: "InventoryReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptAuditLog_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierDebt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryReceiptId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDebt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDebt_InventoryReceipt_InventoryReceiptId",
                        column: x => x.InventoryReceiptId,
                        principalTable: "InventoryReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierDebt_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequestItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseRequestId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ProductQuotationId = table.Column<int>(type: "int", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequestItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItem_ProductQuotations_ProductQuotationId",
                        column: x => x.ProductQuotationId,
                        principalTable: "ProductQuotations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItem_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItem_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItem_PurchaseRequest_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItem_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InventoryReceiptInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryReceiptId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: true),
                    RemainingCount = table.Column<int>(type: "int", nullable: true),
                    ParentOutputInfoId = table.Column<int>(type: "int", nullable: true),
                    PurchaseRequestItemId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfo_InventoryReceipt_InventoryReceiptId",
                        column: x => x.InventoryReceiptId,
                        principalTable: "InventoryReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfo_OutputInfo_ParentOutputInfoId",
                        column: x => x.ParentOutputInfoId,
                        principalTable: "OutputInfo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfo_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfo_PurchaseRequestItem_PurchaseRequestItemId",
                        column: x => x.PurchaseRequestItemId,
                        principalTable: "PurchaseRequestItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequestItemAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseRequestItemId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldQuantity = table.Column<int>(type: "int", nullable: true),
                    NewQuantity = table.Column<int>(type: "int", nullable: true),
                    OldProductVariantId = table.Column<int>(type: "int", nullable: true),
                    NewProductVariantId = table.Column<int>(type: "int", nullable: true),
                    OldProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    NewProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    OldSupplierName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewSupplierName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NewUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequestItemAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItemAuditLog_PurchaseRequestItem_PurchaseRequestItemId",
                        column: x => x.PurchaseRequestItemId,
                        principalTable: "PurchaseRequestItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryReceiptInfoAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryReceiptInfoId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldQuantity = table.Column<int>(type: "int", nullable: true),
                    NewQuantity = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptInfoAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptInfoAuditLog_InventoryReceiptInfo_InventoryReceiptInfoId",
                        column: x => x.InventoryReceiptInfoId,
                        principalTable: "InventoryReceiptInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InventoryReceiptInfoId = table.Column<int>(type: "int", nullable: true),
                    OutputInfoId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantColorId = table.Column<int>(type: "int", nullable: true),
                    VinNumber = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    EngineNumber = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    CurrentOdo = table.Column<double>(type: "float", nullable: false),
                    LastMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceOdo = table.Column<double>(type: "float", nullable: true),
                    ElectronicWarrantyQrCode = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    PurchaseDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ImportPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicle_InventoryReceiptInfo_InventoryReceiptInfoId",
                        column: x => x.InventoryReceiptInfoId,
                        principalTable: "InventoryReceiptInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicle_Lead_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Lead",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vehicle_OutputInfo_OutputInfoId",
                        column: x => x.OutputInfoId,
                        principalTable: "OutputInfo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicle_ProductVariantColor_ProductVariantColorId",
                        column: x => x.ProductVariantColorId,
                        principalTable: "ProductVariantColor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicle_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicle_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicle_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceNumber = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    MaintenanceDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Mileage = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    TechnicianId = table.Column<int>(type: "int", nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PartsCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NextMaintenanceDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NextMaintenanceOdo = table.Column<int>(type: "int", nullable: true),
                    PartsJson = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "EmployeeProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintenanceHistory_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepairOrder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Mileage = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExpectedCompletionTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TechnicianId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PartsCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CompletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairOrder_EmployeeProfile_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "EmployeeProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RepairOrder_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceBooking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TechnicianId = table.Column<int>(type: "int", nullable: true),
                    ScheduledDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CustomerNotes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    TechnicianNotes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CompletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelledDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelledReason = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Review = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBooking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceBooking_EmployeeProfile_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "EmployeeProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceBooking_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceBooking_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceBooking_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OldVinNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewVinNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldEngineNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewEngineNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleAuditLog_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehicleAuditLog_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleDocument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleDocument_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyClaim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimNumber = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    IssueDescription = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    MediaUrls = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    ServiceCenterName = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    ManufacturerClaimNumber = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ManufacturerDecision = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    IsRecall = table.Column<bool>(type: "bit", nullable: false),
                    TotalPartsCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalLaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyClaim_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepairOrderDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepairOrderId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairOrderDetail_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RepairOrderDetail_RepairOrder_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairOrderDetail_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceEvaluation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceBookingId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    Criteria = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Review = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    InternalNotes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    DirectReplyText = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    AdminRepliedById = table.Column<int>(type: "int", nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceEvaluation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceEvaluation_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceEvaluation_ServiceBooking_ServiceBookingId",
                        column: x => x.ServiceBookingId,
                        principalTable: "ServiceBooking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyClaimPart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarrantyClaimId = table.Column<int>(type: "int", nullable: false),
                    PartName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    PartCode = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyClaimPart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyClaimPart_WarrantyClaim_WarrantyClaimId",
                        column: x => x.WarrantyClaimId,
                        principalTable: "WarrantyClaim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PartnerType",
                columns: new[] { "Key", "CreatedAt", "DeletedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { "financial", null, null, null },
                    { "insurance", null, null, null },
                    { "supplier", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "ProductStatus",
                columns: new[] { "Key", "CreatedAt", "DeletedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { "for-sale", null, null, null },
                    { "out-of-business", null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannerAuditLog_BannerId",
                table: "BannerAuditLog",
                column: "BannerId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_ProductVariantId",
                table: "Booking",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAppointment_ConfirmedBy",
                table: "BookingAppointment",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAppointment_ProductVariantId",
                table: "BookingAppointment",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAppointment_Status_AppointmentAt",
                table: "BookingAppointment",
                columns: new[] { "Status", "AppointmentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPolicy_CategoryId",
                table: "CommissionPolicy",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPolicy_ProductId",
                table: "CommissionPolicy",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPolicyAuditLog_PolicyId",
                table: "CommissionPolicyAuditLog",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRecord_EmployeeProfileId",
                table: "CommissionRecord",
                column: "EmployeeProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRecord_OutputId",
                table: "CommissionRecord",
                column: "OutputId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactReply_ContactId",
                table: "ContactReply",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactReply_RepliedById",
                table: "ContactReply",
                column: "RepliedById");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContact_ProcessedBy",
                table: "CustomerContact",
                column: "ProcessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContact_Status_CreatedAt",
                table: "CustomerContact",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContactReply_ContactId_SentAt",
                table: "CustomerContactReply",
                columns: new[] { "ContactId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContactReply_RepliedBy",
                table: "CustomerContactReply",
                column: "RepliedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContactReply_SupportTicketId",
                table: "CustomerContactReply",
                column: "SupportTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedback_ContactId",
                table: "CustomerFeedback",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfile_UserId",
                table: "EmployeeProfile",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLedger_ProductVariantColorId",
                table: "InventoryLedger",
                column: "ProductVariantColorId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLedger_ProductVariantId",
                table: "InventoryLedger",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOnHand_ProductVariantColorId",
                table: "InventoryOnHand",
                column: "ProductVariantColorId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOnHand_ProductVariantId",
                table: "InventoryOnHand",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOnHand_ProductVariantId_ProductVariantColorId_Month_Year",
                table: "InventoryOnHand",
                columns: new[] { "ProductVariantId", "ProductVariantColorId", "Month", "Year" },
                unique: true,
                filter: "[ProductVariantColorId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_ApprovedBy",
                table: "InventoryReceipt",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_ConfirmedBy",
                table: "InventoryReceipt",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_CreatedBy",
                table: "InventoryReceipt",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_PurchaseRequestId",
                table: "InventoryReceipt",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_RejectedBy",
                table: "InventoryReceipt",
                column: "RejectedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_SentBy",
                table: "InventoryReceipt",
                column: "SentBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_SourceOrderId",
                table: "InventoryReceipt",
                column: "SourceOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipt_StatusId",
                table: "InventoryReceipt",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptAuditLog_ChangedById",
                table: "InventoryReceiptAuditLog",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptAuditLog_InventoryReceiptId",
                table: "InventoryReceiptAuditLog",
                column: "InventoryReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfo_InventoryReceiptId",
                table: "InventoryReceiptInfo",
                column: "InventoryReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfo_ParentOutputInfoId",
                table: "InventoryReceiptInfo",
                column: "ParentOutputInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfo_ProductVariantId",
                table: "InventoryReceiptInfo",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfo_PurchaseRequestItemId",
                table: "InventoryReceiptInfo",
                column: "PurchaseRequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptInfoAuditLog_InventoryReceiptInfoId",
                table: "InventoryReceiptInfoAuditLog",
                column: "InventoryReceiptInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_PerformedBy",
                table: "InventoryTransaction",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_ProductVariantId_PerformedAt",
                table: "InventoryTransaction",
                columns: new[] { "ProductVariantId", "PerformedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_UserId",
                table: "Invoice",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplication_ContactId",
                table: "JobApplication",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_KPI_EmployeeProfileId",
                table: "KPI",
                column: "EmployeeProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_AssignedToId",
                table: "Lead",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadActivity_LeadId",
                table: "LeadActivity",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceHistory_TechnicianId",
                table: "MaintenanceHistory",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceHistory_VehicleId",
                table: "MaintenanceHistory",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_News_AuthorId",
                table: "News",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_News_CategoryId",
                table: "News",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_AuthorId",
                table: "NewsArticle",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_PublishedBy",
                table: "NewsArticle",
                column: "PublishedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_Slug",
                table: "NewsArticle",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_Status_PublishedAt",
                table: "NewsArticle",
                columns: new[] { "Status", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NewsComments_NewsId",
                table: "NewsComments",
                column: "NewsId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsComments_UserId",
                table: "NewsComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsProduct_NewsId",
                table: "NewsProduct",
                column: "NewsId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsProduct_ProductVariantColorId",
                table: "NewsProduct",
                column: "ProductVariantColorId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsProduct_ProductVariantId",
                table: "NewsProduct",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Option_Name",
                table: "Option",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OptionValue_OptionId",
                table: "OptionValue",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_ChangedBy",
                table: "OrderStatusHistory",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_OutputId_ChangedAt",
                table: "OrderStatusHistory",
                columns: new[] { "OutputId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Output_BuyerId",
                table: "Output",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Output_CreatedBy",
                table: "Output",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Output_FinishedBy",
                table: "Output",
                column: "FinishedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Output_LeadId",
                table: "Output",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Output_StatusId",
                table: "Output",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OutputInfo_OutputId",
                table: "OutputInfo",
                column: "OutputId");

            migrationBuilder.CreateIndex(
                name: "IX_OutputInfo_ProductVariantColorId",
                table: "OutputInfo",
                column: "ProductVariantColorId");

            migrationBuilder.CreateIndex(
                name: "IX_OutputInfo_ProductVariantId",
                table: "OutputInfo",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDeliveryOrderItems_ParcelDeliveryOrderId",
                table: "ParcelDeliveryOrderItems",
                column: "ParcelDeliveryOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payroll_EmployeeProfileId",
                table: "Payroll",
                column: "EmployeeProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateDossier_OutputId",
                table: "PlateDossier",
                column: "OutputId");

            migrationBuilder.CreateIndex(
                name: "IX_PredefinedOption_Key",
                table: "PredefinedOption",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_BrandId",
                table: "Product",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CategoryId",
                table: "Product",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_StatusId",
                table: "Product",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategory_ParentId",
                table: "ProductCategory",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCollectionPhoto_ProductVariantId",
                table: "ProductCollectionPhoto",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompatibility_BaseProductId",
                table: "ProductCompatibility",
                column: "BaseProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompatibility_CompatibleVehicleModelId",
                table: "ProductCompatibility",
                column: "CompatibleVehicleModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotations_ProductVariantColorId",
                table: "ProductQuotations",
                column: "ProductVariantColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotations_ProductVariantId",
                table: "ProductQuotations",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuotations_SupplierId",
                table: "ProductQuotations",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTechnology_ProductId",
                table: "ProductTechnology",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTechnology_TechnologyId",
                table: "ProductTechnology",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariant_ProductId",
                table: "ProductVariant",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantColor_ProductVariantId",
                table: "ProductVariantColor",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBanner_CreatedBy",
                table: "PromotionBanner",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBanner_IsEnabled_StartDate_EndDate",
                table: "PromotionBanner",
                columns: new[] { "IsEnabled", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionBanner_UpdatedBy",
                table: "PromotionBanner",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequest_ApprovedBy",
                table: "PurchaseRequest",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequest_CreatedBy",
                table: "PurchaseRequest",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequest_RejectedBy",
                table: "PurchaseRequest",
                column: "RejectedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequest_SentBy",
                table: "PurchaseRequest",
                column: "SentBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestAuditLog_ChangedById",
                table: "PurchaseRequestAuditLog",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestAuditLog_PurchaseRequestId",
                table: "PurchaseRequestAuditLog",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_ProductQuotationId",
                table: "PurchaseRequestItem",
                column: "ProductQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_ProductVariantColorId",
                table: "PurchaseRequestItem",
                column: "ProductVariantColorId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_ProductVariantId",
                table: "PurchaseRequestItem",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_PurchaseRequestId",
                table: "PurchaseRequestItem",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItem_SupplierId",
                table: "PurchaseRequestItem",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItemAuditLog_PurchaseRequestItemId",
                table: "PurchaseRequestItemAuditLog",
                column: "PurchaseRequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrder_TechnicianId",
                table: "RepairOrder",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrder_VehicleId",
                table: "RepairOrder",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderDetail_ProductVariantId",
                table: "RepairOrderDetail",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderDetail_RepairOrderId",
                table: "RepairOrderDetail",
                column: "RepairOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderDetail_ServiceId",
                table: "RepairOrderDetail",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesContracts_CustomerId",
                table: "SalesContracts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesContracts_OutputId",
                table: "SalesContracts",
                column: "OutputId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBooking_CustomerId",
                table: "ServiceBooking",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBooking_ServiceId",
                table: "ServiceBooking",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBooking_TechnicianId",
                table: "ServiceBooking",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBooking_VehicleId",
                table: "ServiceBooking",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEvaluation_ContactId",
                table: "ServiceEvaluation",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEvaluation_ServiceBookingId",
                table: "ServiceEvaluation",
                column: "ServiceBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CategoryId",
                table: "Services",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_PartnerTypeId",
                table: "Supplier",
                column: "PartnerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_StatusId",
                table: "Supplier",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContact_SupplierId",
                table: "SupplierContact",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContractAuditLog_SupplierContractId",
                table: "SupplierContractAuditLog",
                column: "SupplierContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContractItem_ProductVariantId",
                table: "SupplierContractItem",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContractItem_SupplierContractId",
                table: "SupplierContractItem",
                column: "SupplierContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContracts_ParentContractId",
                table: "SupplierContracts",
                column: "ParentContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContracts_SupplierId",
                table: "SupplierContracts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebt_InventoryReceiptId",
                table: "SupplierDebt",
                column: "InventoryReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebt_SupplierId",
                table: "SupplierDebt",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebtLog_CreatedById",
                table: "SupplierDebtLog",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebtLog_SupplierId",
                table: "SupplierDebtLog",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebtLogImages_SupplierDebtLogId",
                table: "SupplierDebtLogImages",
                column: "SupplierDebtLogId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDebtSettlements_SupplierId",
                table: "SupplierDebtSettlements",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequest_AssignedUserId",
                table: "SupportRequest",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequest_ContactId",
                table: "SupportRequest",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_AssignedAdminId",
                table: "SupportTicket",
                column: "AssignedAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicket_CustomerId",
                table: "SupportTicket",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Technologies_BrandId",
                table: "Technologies",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Technologies_CategoryId",
                table: "Technologies",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyImages_TechnologyId",
                table: "TechnologyImages",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_ApplicationUserId",
                table: "UserRoles",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VariantOptionValue_OptionValueId",
                table: "VariantOptionValue",
                column: "OptionValueId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantOptionValue_VariantId",
                table: "VariantOptionValue",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_InventoryReceiptInfoId",
                table: "Vehicle",
                column: "InventoryReceiptInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_LeadId",
                table: "Vehicle",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_OutputInfoId",
                table: "Vehicle",
                column: "OutputInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_ProductId",
                table: "Vehicle",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_ProductVariantColorId",
                table: "Vehicle",
                column: "ProductVariantColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_ProductVariantId",
                table: "Vehicle",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_UserId",
                table: "Vehicle",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAuditLog_ChangedById",
                table: "VehicleAuditLog",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAuditLog_VehicleId",
                table: "VehicleAuditLog",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleDocument_VehicleId",
                table: "VehicleDocument",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaim_VehicleId",
                table: "WarrantyClaim",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaimPart_WarrantyClaimId",
                table: "WarrantyClaimPart",
                column: "WarrantyClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannerAuditLog");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "BookingAppointment");

            migrationBuilder.DropTable(
                name: "CarrierPartners");

            migrationBuilder.DropTable(
                name: "CommissionPolicyAuditLog");

            migrationBuilder.DropTable(
                name: "CommissionRecord");

            migrationBuilder.DropTable(
                name: "ContactReply");

            migrationBuilder.DropTable(
                name: "CurrentUnreconciledCods");

            migrationBuilder.DropTable(
                name: "CustomerContactReply");

            migrationBuilder.DropTable(
                name: "CustomerFeedback");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "FinanceContracts");

            migrationBuilder.DropTable(
                name: "InventoryLedger");

            migrationBuilder.DropTable(
                name: "InventoryOnHand");

            migrationBuilder.DropTable(
                name: "InventoryReceiptAuditLog");

            migrationBuilder.DropTable(
                name: "InventoryReceiptInfoAuditLog");

            migrationBuilder.DropTable(
                name: "InventoryTransaction");

            migrationBuilder.DropTable(
                name: "Invoice");

            migrationBuilder.DropTable(
                name: "JobApplication");

            migrationBuilder.DropTable(
                name: "KPI");

            migrationBuilder.DropTable(
                name: "LeadActivity");

            migrationBuilder.DropTable(
                name: "MaintenanceHistory");

            migrationBuilder.DropTable(
                name: "MediaFiles");

            migrationBuilder.DropTable(
                name: "NewsArticle");

            migrationBuilder.DropTable(
                name: "NewsComments");

            migrationBuilder.DropTable(
                name: "NewsProduct");

            migrationBuilder.DropTable(
                name: "OrderLogistics");

            migrationBuilder.DropTable(
                name: "OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "ParcelDeliveryOrderItems");

            migrationBuilder.DropTable(
                name: "Payroll");

            migrationBuilder.DropTable(
                name: "PlateDossier");

            migrationBuilder.DropTable(
                name: "ProductCollectionPhoto");

            migrationBuilder.DropTable(
                name: "ProductCompatibility");

            migrationBuilder.DropTable(
                name: "ProductTechnology");

            migrationBuilder.DropTable(
                name: "PromotionBanner");

            migrationBuilder.DropTable(
                name: "PurchaseRequestAuditLog");

            migrationBuilder.DropTable(
                name: "PurchaseRequestItemAuditLog");

            migrationBuilder.DropTable(
                name: "RepairOrderDetail");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SalesContracts");

            migrationBuilder.DropTable(
                name: "ServiceEvaluation");

            migrationBuilder.DropTable(
                name: "Setting");

            migrationBuilder.DropTable(
                name: "SupplierContact");

            migrationBuilder.DropTable(
                name: "SupplierContractAuditLog");

            migrationBuilder.DropTable(
                name: "SupplierContractItem");

            migrationBuilder.DropTable(
                name: "SupplierDebt");

            migrationBuilder.DropTable(
                name: "SupplierDebtLogImages");

            migrationBuilder.DropTable(
                name: "SupplierDebtSettlements");

            migrationBuilder.DropTable(
                name: "SupplierFinances");

            migrationBuilder.DropTable(
                name: "SupportRequest");

            migrationBuilder.DropTable(
                name: "TechnologyImages");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "VariantOptionValue");

            migrationBuilder.DropTable(
                name: "VehicleAuditLog");

            migrationBuilder.DropTable(
                name: "VehicleDocument");

            migrationBuilder.DropTable(
                name: "WarrantyClaimPart");

            migrationBuilder.DropTable(
                name: "Banner");

            migrationBuilder.DropTable(
                name: "CommissionPolicy");

            migrationBuilder.DropTable(
                name: "CustomerContact");

            migrationBuilder.DropTable(
                name: "SupportTicket");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "ParcelDeliveryOrders");

            migrationBuilder.DropTable(
                name: "RepairOrder");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "ServiceBooking");

            migrationBuilder.DropTable(
                name: "SupplierContracts");

            migrationBuilder.DropTable(
                name: "SupplierDebtLog");

            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropTable(
                name: "Technologies");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "OptionValue");

            migrationBuilder.DropTable(
                name: "WarrantyClaim");

            migrationBuilder.DropTable(
                name: "NewsCategory");

            migrationBuilder.DropTable(
                name: "EmployeeProfile");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "TechnologyCategories");

            migrationBuilder.DropTable(
                name: "Option");

            migrationBuilder.DropTable(
                name: "Vehicle");

            migrationBuilder.DropTable(
                name: "ServiceCategories");

            migrationBuilder.DropTable(
                name: "PredefinedOption");

            migrationBuilder.DropTable(
                name: "InventoryReceiptInfo");

            migrationBuilder.DropTable(
                name: "InventoryReceipt");

            migrationBuilder.DropTable(
                name: "OutputInfo");

            migrationBuilder.DropTable(
                name: "PurchaseRequestItem");

            migrationBuilder.DropTable(
                name: "InventoryReceiptStatus");

            migrationBuilder.DropTable(
                name: "Output");

            migrationBuilder.DropTable(
                name: "ProductQuotations");

            migrationBuilder.DropTable(
                name: "PurchaseRequest");

            migrationBuilder.DropTable(
                name: "Lead");

            migrationBuilder.DropTable(
                name: "OutputStatus");

            migrationBuilder.DropTable(
                name: "ProductVariantColor");

            migrationBuilder.DropTable(
                name: "Supplier");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ProductVariant");

            migrationBuilder.DropTable(
                name: "PartnerType");

            migrationBuilder.DropTable(
                name: "SupplierStatus");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Brand");

            migrationBuilder.DropTable(
                name: "ProductCategory");

            migrationBuilder.DropTable(
                name: "ProductStatus");
        }
    }
}
