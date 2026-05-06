using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.PostgreSqlMigrations
{
    /// <inheritdoc />
    public partial class CRMDatabaseandAPIExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    LinkUrl = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banner", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Brand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brand", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    InternalNote = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "InputStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputStatus", x => x.Key);
                });
            migrationBuilder.CreateTable(
                name: "Lead",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Tier = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lead", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StorageType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OriginalFileName = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileExtension = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "varchar(255)", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    AuthorName = table.Column<string>(type: "text", nullable: true),
                    PublishedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    MetaTitle = table.Column<string>(type: "text", nullable: true),
                    MetaDescription = table.Column<string>(type: "text", nullable: true),
                    MetaKeywords = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "OutputStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutputStatus", x => x.Key);
                });
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "PredefinedOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    MaxPurchaseQuantity = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategory", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "ProductStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStatus", x => x.Key);
                });
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Key);
                });
            migrationBuilder.CreateTable(
                name: "SupplierStatus",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierStatus", x => x.Key);
                });
            migrationBuilder.CreateTable(
                name: "TechnologyCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnologyCategories", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "LeadActivity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeadId = table.Column<int>(type: "integer", nullable: false),
                    ActivityType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                name: "Vehicle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeadId = table.Column<int>(type: "integer", nullable: false),
                    VinNumber = table.Column<string>(type: "text", nullable: false),
                    EngineNumber = table.Column<string>(type: "text", nullable: false),
                    LicensePlate = table.Column<string>(type: "text", nullable: false),
                    PurchaseDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicle_Lead_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Option",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ShortDescription = table.Column<string>(type: "text", nullable: true),
                    MetaTitle = table.Column<string>(type: "text", nullable: true),
                    MetaDescription = table.Column<string>(type: "text", nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    StatusId = table.Column<string>(type: "text", nullable: true),
                    BrandId = table.Column<int>(type: "integer", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric", nullable: true),
                    Dimensions = table.Column<string>(type: "text", nullable: true),
                    Wheelbase = table.Column<string>(type: "text", nullable: true),
                    SeatHeight = table.Column<decimal>(type: "numeric", nullable: true),
                    GroundClearance = table.Column<decimal>(type: "numeric", nullable: true),
                    FuelCapacity = table.Column<decimal>(type: "numeric", nullable: true),
                    TireSize = table.Column<string>(type: "text", nullable: true),
                    FrontSuspension = table.Column<string>(type: "text", nullable: true),
                    RearSuspension = table.Column<string>(type: "text", nullable: true),
                    EngineType = table.Column<string>(type: "text", nullable: true),
                    MaxPower = table.Column<string>(type: "text", nullable: true),
                    OilCapacity = table.Column<decimal>(type: "numeric", nullable: true),
                    FuelConsumption = table.Column<string>(type: "text", nullable: true),
                    TransmissionType = table.Column<string>(type: "text", nullable: true),
                    StarterSystem = table.Column<string>(type: "text", nullable: true),
                    MaxTorque = table.Column<string>(type: "text", nullable: true),
                    Displacement = table.Column<decimal>(type: "numeric", nullable: true),
                    BoreStroke = table.Column<string>(type: "text", nullable: true),
                    CompressionRatio = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Highlights = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
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
                name: "Supplier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    StatusId = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    TaxIdentificationNumber = table.Column<string>(type: "varchar(20)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.Id);
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DefaultTitle = table.Column<string>(type: "text", nullable: true),
                    DefaultDescription = table.Column<string>(type: "text", nullable: true),
                    DefaultImageUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technologies", x => x.Id);
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContactId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    RepliedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Output",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerName = table.Column<string>(type: "text", nullable: true),
                    CustomerAddress = table.Column<string>(type: "text", nullable: true),
                    CustomerPhone = table.Column<string>(type: "text", nullable: true),
                    LastStatusChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    FinishedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusId = table.Column<string>(type: "text", nullable: true),
                    PaymentMethod = table.Column<string>(type: "text", nullable: true),
                    TransactionId = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    DepositRatio = table.Column<int>(type: "integer", nullable: true),
                    PaymentUrl = table.Column<string>(type: "text", nullable: true),
                    PaymentCode = table.Column<string>(type: "text", nullable: true),
                    PaymentExpiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Output", x => x.id);
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
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
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
                name: "MaintenanceHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    MaintenanceDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Mileage = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceHistory_Vehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "VehicleDocument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                name: "OptionValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OptionId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ColorCode = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                name: "ProductVariant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    UrlSlug = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    VersionName = table.Column<string>(type: "text", nullable: true),
                    ColorName = table.Column<string>(type: "text", nullable: true),
                    ColorCode = table.Column<string>(type: "text", nullable: true),
                    SKU = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    CitizenID = table.Column<string>(type: "varchar(20)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                name: "ProductTechnology",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    TechnologyId = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CustomTitle = table.Column<string>(type: "text", nullable: true),
                    CustomDescription = table.Column<string>(type: "text", nullable: true),
                    CustomImageUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TechnologyId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                name: "Input",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InputDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    StatusId = table.Column<string>(type: "text", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceOrderId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Input", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Input_InputStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "InputStatus",
                        principalColumn: "Key");
                    table.ForeignKey(
                        name: "FK_Input_Output_SourceOrderId",
                        column: x => x.SourceOrderId,
                        principalTable: "Output",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Input_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Input_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Input_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });
            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: true),
                    PreferredDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    BookingType = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                name: "OutputInfo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductVarientId = table.Column<int>(type: "integer", nullable: true),
                    Count = table.Column<int>(type: "integer", nullable: true),
                    OutputId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CostPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                        name: "FK_OutputInfo_ProductVariant_ProductVarientId",
                        column: x => x.ProductVarientId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id");
                });
            migrationBuilder.CreateTable(
                name: "ProductCollectionPhoto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductVariantId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                name: "VariantOptionValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VariantId = table.Column<int>(type: "integer", nullable: false),
                    OptionValueId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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
                name: "InputInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InputId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    Count = table.Column<int>(type: "integer", nullable: true),
                    InputPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RemainingCount = table.Column<int>(type: "integer", nullable: true),
                    ParentOutputInfoId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputInfo_Input_InputId",
                        column: x => x.InputId,
                        principalTable: "Input",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InputInfo_OutputInfo_ParentOutputInfoId",
                        column: x => x.ParentOutputInfoId,
                        principalTable: "OutputInfo",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_InputInfo_ProductVariant_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id");
                });
            migrationBuilder.InsertData(
                table: "InputStatus",
                columns: new[] { "Key", "CreatedAt", "DeletedAt", "UpdatedAt" },
                values: new object[,]
                { { "cancelled", null, null, null }, { "finished", null, null, null }, { "working", null, null, null } });
            migrationBuilder.InsertData(
                table: "OutputStatus",
                columns: new[] { "Key", "CreatedAt", "DeletedAt", "UpdatedAt" },
                values: new object[,]
                {
                {
                    "cancelled",
                    null,
                    null,
                    null
                },
                {
                    "delivered",
                    null,
                    null,
                    null
                },
                {
                    "pending",
                    null,
                    null,
                    null
                },
                {
                    "processing",
                    null,
                    null,
                    null
                },
                {
                    "shipped",
                    null,
                    null,
                    null
                }
                });
            migrationBuilder.InsertData(
                table: "ProductStatus",
                columns: new[] { "Key", "CreatedAt", "DeletedAt", "UpdatedAt" },
                values: new object[,] { { "for-sale", null, null, null }, { "out-of-business", null, null, null } });
            migrationBuilder.CreateIndex(
                name: "IX_Booking_ProductVariantId",
                table: "Booking",
                column: "ProductVariantId");
            migrationBuilder.CreateIndex(name: "IX_ContactReply_ContactId", table: "ContactReply", column: "ContactId");
            migrationBuilder.CreateIndex(
                name: "IX_ContactReply_RepliedById",
                table: "ContactReply",
                column: "RepliedById");
            migrationBuilder.CreateIndex(name: "IX_Input_ConfirmedBy", table: "Input", column: "ConfirmedBy");
            migrationBuilder.CreateIndex(name: "IX_Input_CreatedBy", table: "Input", column: "CreatedBy");
            migrationBuilder.CreateIndex(name: "IX_Input_SourceOrderId", table: "Input", column: "SourceOrderId");
            migrationBuilder.CreateIndex(name: "IX_Input_StatusId", table: "Input", column: "StatusId");
            migrationBuilder.CreateIndex(name: "IX_Input_SupplierId", table: "Input", column: "SupplierId");
            migrationBuilder.CreateIndex(name: "IX_InputInfo_InputId", table: "InputInfo", column: "InputId");
            migrationBuilder.CreateIndex(
                name: "IX_InputInfo_ParentOutputInfoId",
                table: "InputInfo",
                column: "ParentOutputInfoId");
            migrationBuilder.CreateIndex(name: "IX_InputInfo_ProductId", table: "InputInfo", column: "ProductId");
            migrationBuilder.CreateIndex(name: "IX_LeadActivity_LeadId", table: "LeadActivity", column: "LeadId");
            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceHistory_VehicleId",
                table: "MaintenanceHistory",
                column: "VehicleId");
            migrationBuilder.CreateIndex(name: "IX_Option_Name", table: "Option", column: "Name");
            migrationBuilder.CreateIndex(name: "IX_OptionValue_OptionId", table: "OptionValue", column: "OptionId");
            migrationBuilder.CreateIndex(name: "IX_Output_BuyerId", table: "Output", column: "BuyerId");
            migrationBuilder.CreateIndex(name: "IX_Output_CreatedBy", table: "Output", column: "CreatedBy");
            migrationBuilder.CreateIndex(name: "IX_Output_FinishedBy", table: "Output", column: "FinishedBy");
            migrationBuilder.CreateIndex(name: "IX_Output_StatusId", table: "Output", column: "StatusId");
            migrationBuilder.CreateIndex(name: "IX_OutputInfo_OutputId", table: "OutputInfo", column: "OutputId");
            migrationBuilder.CreateIndex(
                name: "IX_OutputInfo_ProductVarientId",
                table: "OutputInfo",
                column: "ProductVarientId");
            migrationBuilder.CreateIndex(
                name: "IX_PredefinedOption_Key",
                table: "PredefinedOption",
                column: "Key",
                unique: true);
            migrationBuilder.CreateIndex(name: "IX_Product_BrandId", table: "Product", column: "BrandId");
            migrationBuilder.CreateIndex(name: "IX_Product_CategoryId", table: "Product", column: "CategoryId");
            migrationBuilder.CreateIndex(name: "IX_Product_StatusId", table: "Product", column: "StatusId");
            migrationBuilder.CreateIndex(
                name: "IX_ProductCollectionPhoto_ProductVariantId",
                table: "ProductCollectionPhoto",
                column: "ProductVariantId");
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
            migrationBuilder.CreateIndex(name: "IX_RoleClaims_RoleId", table: "RoleClaims", column: "RoleId");
            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");
            migrationBuilder.CreateIndex(name: "RoleNameIndex", table: "Roles", column: "NormalizedName", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Supplier_StatusId", table: "Supplier", column: "StatusId");
            migrationBuilder.CreateIndex(
                name: "IX_SupplierContact_SupplierId",
                table: "SupplierContact",
                column: "SupplierId");
            migrationBuilder.CreateIndex(
                name: "IX_Technologies_CategoryId",
                table: "Technologies",
                column: "CategoryId");
            migrationBuilder.CreateIndex(
                name: "IX_TechnologyImages_TechnologyId",
                table: "TechnologyImages",
                column: "TechnologyId");
            migrationBuilder.CreateIndex(name: "IX_UserClaims_UserId", table: "UserClaims", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_UserLogins_UserId", table: "UserLogins", column: "UserId");
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_ApplicationUserId",
                table: "UserRoles",
                column: "ApplicationUserId");
            migrationBuilder.CreateIndex(name: "IX_UserRoles_RoleId", table: "UserRoles", column: "RoleId");
            migrationBuilder.CreateIndex(name: "EmailIndex", table: "Users", column: "NormalizedEmail");
            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);
            migrationBuilder.CreateIndex(
                name: "IX_VariantOptionValue_OptionValueId",
                table: "VariantOptionValue",
                column: "OptionValueId");
            migrationBuilder.CreateIndex(
                name: "IX_VariantOptionValue_VariantId",
                table: "VariantOptionValue",
                column: "VariantId");
            migrationBuilder.CreateIndex(name: "IX_Vehicle_LeadId", table: "Vehicle", column: "LeadId");
            migrationBuilder.CreateIndex(
                name: "IX_VehicleDocument_VehicleId",
                table: "VehicleDocument",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Banner");
            migrationBuilder.DropTable(name: "Booking");
            migrationBuilder.DropTable(name: "ContactReply");
            migrationBuilder.DropTable(name: "InputInfo");
            migrationBuilder.DropTable(name: "LeadActivity");
            migrationBuilder.DropTable(name: "MaintenanceHistory");
            migrationBuilder.DropTable(name: "MediaFiles");
            migrationBuilder.DropTable(name: "News");
            migrationBuilder.DropTable(name: "ProductCollectionPhoto");
            migrationBuilder.DropTable(name: "ProductTechnology");
            migrationBuilder.DropTable(name: "RoleClaims");
            migrationBuilder.DropTable(name: "RolePermissions");
            migrationBuilder.DropTable(name: "Setting");
            migrationBuilder.DropTable(name: "SupplierContact");
            migrationBuilder.DropTable(name: "TechnologyImages");
            migrationBuilder.DropTable(name: "UserClaims");
            migrationBuilder.DropTable(name: "UserLogins");
            migrationBuilder.DropTable(name: "UserRoles");
            migrationBuilder.DropTable(name: "UserTokens");
            migrationBuilder.DropTable(name: "VariantOptionValue");
            migrationBuilder.DropTable(name: "VehicleDocument");
            migrationBuilder.DropTable(name: "Contact");
            migrationBuilder.DropTable(name: "Input");
            migrationBuilder.DropTable(name: "OutputInfo");
            migrationBuilder.DropTable(name: "Permissions");
            migrationBuilder.DropTable(name: "Technologies");
            migrationBuilder.DropTable(name: "Roles");
            migrationBuilder.DropTable(name: "OptionValue");
            migrationBuilder.DropTable(name: "Vehicle");
            migrationBuilder.DropTable(name: "InputStatus");
            migrationBuilder.DropTable(name: "Supplier");
            migrationBuilder.DropTable(name: "Output");
            migrationBuilder.DropTable(name: "ProductVariant");
            migrationBuilder.DropTable(name: "TechnologyCategories");
            migrationBuilder.DropTable(name: "Option");
            migrationBuilder.DropTable(name: "Lead");
            migrationBuilder.DropTable(name: "SupplierStatus");
            migrationBuilder.DropTable(name: "OutputStatus");
            migrationBuilder.DropTable(name: "Users");
            migrationBuilder.DropTable(name: "Product");
            migrationBuilder.DropTable(name: "PredefinedOption");
            migrationBuilder.DropTable(name: "Brand");
            migrationBuilder.DropTable(name: "ProductCategory");
            migrationBuilder.DropTable(name: "ProductStatus");
        }
    }
}
