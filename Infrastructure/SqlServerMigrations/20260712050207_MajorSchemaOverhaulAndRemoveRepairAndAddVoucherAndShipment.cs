using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class MajorSchemaOverhaulAndRemoveRepairAndAddVoucherAndShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory");
            migrationBuilder.DropTable(name: "PlateDossier");
            migrationBuilder.DropTable(name: "RepairOrderDetail");
            migrationBuilder.DropTable(name: "RepairOrder");
            migrationBuilder.AlterColumn<string>(
                name: "VehicleInfo",
                table: "WorkshopPayment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "SourceType",
                table: "WorkshopPayment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)");
            migrationBuilder.AlterColumn<string>(
                name: "ServiceDescription",
                table: "WorkshopPayment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "WorkshopPayment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)");
            migrationBuilder.AlterColumn<string>(
                name: "PaymentNumber",
                table: "WorkshopPayment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");
            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "WorkshopPayment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)");
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "WorkshopPayment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "CustomerPhone",
                table: "WorkshopPayment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)");
            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "WorkshopPayment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");
            migrationBuilder.AlterColumn<string>(
                name: "PartName",
                table: "WarrantyClaimPart",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)");
            migrationBuilder.AlterColumn<string>(
                name: "PartCode",
                table: "WarrantyClaimPart",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");
            migrationBuilder.AlterColumn<string>(
                name: "ServiceCenterName",
                table: "WarrantyClaim",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "MediaUrls",
                table: "WarrantyClaim",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "ManufacturerDecision",
                table: "WarrantyClaim",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "ManufacturerClaimNumber",
                table: "WarrantyClaim",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "IssueDescription",
                table: "WarrantyClaim",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");
            migrationBuilder.AlterColumn<string>(
                name: "ClaimNumber",
                table: "WarrantyClaim",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Users', 'PasswordResetToken') IS NULL
                BEGIN
                    ALTER TABLE [Users] ADD [PasswordResetToken] nvarchar(max) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Users', 'PasswordResetTokenExpiry') IS NULL
                BEGIN
                    ALTER TABLE [Users] ADD [PasswordResetTokenExpiry] datetimeoffset NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Product', 'DescriptionJson') IS NULL
                BEGIN
                    ALTER TABLE [Product] ADD [DescriptionJson] nvarchar(max) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Product', 'MetaDescriptionJson') IS NULL
                BEGIN
                    ALTER TABLE [Product] ADD [MetaDescriptionJson] nvarchar(max) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Product', 'MetaTitleJson') IS NULL
                BEGIN
                    ALTER TABLE [Product] ADD [MetaTitleJson] nvarchar(max) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Product', 'NameJson') IS NULL
                BEGIN
                    ALTER TABLE [Product] ADD [NameJson] nvarchar(max) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Product', 'ShortDescriptionJson') IS NULL
                BEGIN
                    ALTER TABLE [Product] ADD [ShortDescriptionJson] nvarchar(max) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'BudgetCode') IS NULL
                BEGIN
                    ALTER TABLE [Output] ADD [BudgetCode] nvarchar(50) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'CompanyAddress') IS NULL
                BEGIN
                    ALTER TABLE [Output] ADD [CompanyAddress] nvarchar(500) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'CompanyEmail') IS NULL
                BEGIN
                    ALTER TABLE [Output] ADD [CompanyEmail] nvarchar(150) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'CompanyName') IS NULL
                BEGIN
                    ALTER TABLE [Output] ADD [CompanyName] nvarchar(200) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'CompanyTaxCode') IS NULL
                BEGIN
                    ALTER TABLE [Output] ADD [CompanyTaxCode] nvarchar(50) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'IsCompanyInvoice') IS NULL
                BEGIN
                    ALTER TABLE [Output] ADD [IsCompanyInvoice] bit NOT NULL DEFAULT 0;
                END
            ");
            migrationBuilder.AlterColumn<string>(
                name: "PartsJson",
                table: "MaintenanceHistory",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "MaintenanceNumber",
                table: "MaintenanceHistory",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MaintenanceHistory",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Brand', 'DescriptionJson') IS NULL
                BEGIN
                    ALTER TABLE [Brand] ADD [DescriptionJson] nvarchar(max) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Brand', 'NameJson') IS NULL
                BEGIN
                    ALTER TABLE [Brand] ADD [NameJson] nvarchar(max) NULL;
                END
            ");
            migrationBuilder.Sql(@"
                IF OBJECT_ID('ProductCategoryTranslations', 'U') IS NULL
                BEGIN
                    CREATE TABLE [ProductCategoryTranslations] (
                        [Id] int NOT NULL IDENTITY,
                        [ProductCategoryId] int NOT NULL,
                        [LanguageCode] nvarchar(max) NOT NULL,
                        [Name] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NULL,
                        [CreatedAt] datetimeoffset NULL,
                        [UpdatedAt] datetimeoffset NULL,
                        [DeletedAt] datetimeoffset NULL,
                        CONSTRAINT [PK_ProductCategoryTranslations] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ProductCategoryTranslations_ProductCategory_ProductCategoryId] FOREIGN KEY ([ProductCategoryId]) REFERENCES [ProductCategory] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_ProductCategoryTranslations_ProductCategoryId] ON [ProductCategoryTranslations] ([ProductCategoryId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID('Shipments', 'U') IS NULL
                BEGIN
                    CREATE TABLE [Shipments] (
                        [Id] int NOT NULL IDENTITY,
                        [Status] int NOT NULL,
                        [TrackingNumber] nvarchar(max) NOT NULL,
                        [Carrier] nvarchar(max) NOT NULL,
                        [CustomerName] nvarchar(max) NOT NULL,
                        [CustomerPhone] nvarchar(max) NOT NULL,
                        [CodAmount] decimal(18,2) NOT NULL,
                        [ShippingCost] decimal(18,2) NOT NULL,
                        [DeliveredAt] datetimeoffset NULL,
                        [OriginAddress] nvarchar(max) NOT NULL,
                        [DestinationAddress] nvarchar(max) NOT NULL,
                        [OriginLatitude] float NULL,
                        [OriginLongitude] float NULL,
                        [DestinationLatitude] float NULL,
                        [DestinationLongitude] float NULL,
                        [Type] nvarchar(max) NOT NULL,
                        [OutputId] int NULL,
                        [CreatedAt] datetimeoffset NULL,
                        [UpdatedAt] datetimeoffset NULL,
                        [DeletedAt] datetimeoffset NULL,
                        CONSTRAINT [PK_Shipments] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Shipments_Output_OutputId] FOREIGN KEY ([OutputId]) REFERENCES [Output] ([id])
                    );
                    CREATE INDEX [IX_Shipments_OutputId] ON [Shipments] ([OutputId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID('Vouchers', 'U') IS NULL
                BEGIN
                    CREATE TABLE [Vouchers] (
                        [Id] int NOT NULL IDENTITY,
                        [Code] nvarchar(450) NOT NULL,
                        [Name] nvarchar(max) NOT NULL,
                        [ApplyFor] int NOT NULL,
                        [Channel] int NOT NULL,
                        [Type] int NOT NULL,
                        [DiscountType] int NOT NULL,
                        [DiscountValue] decimal(18,2) NOT NULL,
                        [MaxDiscountAmount] decimal(18,2) NULL,
                        [ValidFrom] datetime2 NOT NULL,
                        [ValidTo] datetime2 NOT NULL,
                        [CreatedAt] datetimeoffset NULL,
                        [UpdatedAt] datetimeoffset NULL,
                        [DeletedAt] datetimeoffset NULL,
                        CONSTRAINT [PK_Vouchers] PRIMARY KEY ([Id])
                    );
                    CREATE UNIQUE INDEX [IX_Vouchers_Code] ON [Vouchers] ([Code]);
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID('ShipmentItems', 'U') IS NULL
                BEGIN
                    CREATE TABLE [ShipmentItems] (
                        [Id] int NOT NULL IDENTITY,
                        [ShipmentId] int NOT NULL,
                        [ProductVariantId] int NULL,
                        [ProductVariantColorId] int NULL,
                        [Quantity] int NOT NULL,
                        [CreatedAt] datetimeoffset NULL,
                        [UpdatedAt] datetimeoffset NULL,
                        [DeletedAt] datetimeoffset NULL,
                        CONSTRAINT [PK_ShipmentItems] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ShipmentItems_ProductVariantColor_ProductVariantColorId] FOREIGN KEY ([ProductVariantColorId]) REFERENCES [ProductVariantColor] ([Id]),
                        CONSTRAINT [FK_ShipmentItems_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]),
                        CONSTRAINT [FK_ShipmentItems_Shipments_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipments] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_ShipmentItems_ProductVariantColorId] ON [ShipmentItems] ([ProductVariantColorId]);
                    CREATE INDEX [IX_ShipmentItems_ProductVariantId] ON [ShipmentItems] ([ProductVariantId]);
                    CREATE INDEX [IX_ShipmentItems_ShipmentId] ON [ShipmentItems] ([ShipmentId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID('VoucherLeads', 'U') IS NULL
                BEGIN
                    CREATE TABLE [VoucherLeads] (
                        [VoucherId] int NOT NULL,
                        [LeadId] int NOT NULL,
                        CONSTRAINT [PK_VoucherLeads] PRIMARY KEY ([VoucherId], [LeadId]),
                        CONSTRAINT [FK_VoucherLeads_Lead_LeadId] FOREIGN KEY ([LeadId]) REFERENCES [Lead] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_VoucherLeads_Vouchers_VoucherId] FOREIGN KEY ([VoucherId]) REFERENCES [Vouchers] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_VoucherLeads_LeadId] ON [VoucherLeads] ([LeadId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MaintenanceHistory_EmployeeProfile_TechnicianId' AND parent_object_id = OBJECT_ID('MaintenanceHistory'))
                BEGIN
                    ALTER TABLE [MaintenanceHistory] ADD CONSTRAINT [FK_MaintenanceHistory_EmployeeProfile_TechnicianId] FOREIGN KEY ([TechnicianId]) REFERENCES [EmployeeProfile] ([Id]) ON DELETE SET NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory");
            migrationBuilder.Sql(@"
                IF OBJECT_ID('VoucherLeads', 'U') IS NOT NULL DROP TABLE [VoucherLeads];
                IF OBJECT_ID('ShipmentItems', 'U') IS NOT NULL DROP TABLE [ShipmentItems];
                IF OBJECT_ID('Shipments', 'U') IS NOT NULL DROP TABLE [Shipments];
                IF OBJECT_ID('Vouchers', 'U') IS NOT NULL DROP TABLE [Vouchers];
                IF OBJECT_ID('ProductCategoryTranslations', 'U') IS NOT NULL DROP TABLE [ProductCategoryTranslations];
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Users', 'PasswordResetToken') IS NOT NULL
                BEGIN
                    ALTER TABLE [Users] DROP COLUMN [PasswordResetToken];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Users', 'PasswordResetTokenExpiry') IS NOT NULL
                BEGIN
                    ALTER TABLE [Users] DROP COLUMN [PasswordResetTokenExpiry];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Product', 'DescriptionJson') IS NOT NULL
                BEGIN
                    ALTER TABLE [Product] DROP COLUMN [DescriptionJson];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Product', 'MetaDescriptionJson') IS NOT NULL
                BEGIN
                    ALTER TABLE [Product] DROP COLUMN [MetaDescriptionJson];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Product', 'MetaTitleJson') IS NOT NULL
                BEGIN
                    ALTER TABLE [Product] DROP COLUMN [MetaTitleJson];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Product', 'NameJson') IS NOT NULL
                BEGIN
                    ALTER TABLE [Product] DROP COLUMN [NameJson];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Product', 'ShortDescriptionJson') IS NOT NULL
                BEGIN
                    ALTER TABLE [Product] DROP COLUMN [ShortDescriptionJson];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'BudgetCode') IS NOT NULL
                BEGIN
                    ALTER TABLE [Output] DROP COLUMN [BudgetCode];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'CompanyAddress') IS NOT NULL
                BEGIN
                    ALTER TABLE [Output] DROP COLUMN [CompanyAddress];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'CompanyEmail') IS NOT NULL
                BEGIN
                    ALTER TABLE [Output] DROP COLUMN [CompanyEmail];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'CompanyName') IS NOT NULL
                BEGIN
                    ALTER TABLE [Output] DROP COLUMN [CompanyName];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'CompanyTaxCode') IS NOT NULL
                BEGIN
                    ALTER TABLE [Output] DROP COLUMN [CompanyTaxCode];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Output', 'IsCompanyInvoice') IS NOT NULL
                BEGIN
                    ALTER TABLE [Output] DROP COLUMN [IsCompanyInvoice];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Brand', 'DescriptionJson') IS NOT NULL
                BEGIN
                    ALTER TABLE [Brand] DROP COLUMN [DescriptionJson];
                END
            ");
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Brand', 'NameJson') IS NOT NULL
                BEGIN
                    ALTER TABLE [Brand] DROP COLUMN [NameJson];
                END
            ");
            migrationBuilder.AlterColumn<string>(
                name: "VehicleInfo",
                table: "WorkshopPayment",
                type: "nvarchar(200)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "SourceType",
                table: "WorkshopPayment",
                type: "nvarchar(30)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.AlterColumn<string>(
                name: "ServiceDescription",
                table: "WorkshopPayment",
                type: "nvarchar(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "WorkshopPayment",
                type: "nvarchar(30)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.AlterColumn<string>(
                name: "PaymentNumber",
                table: "WorkshopPayment",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "WorkshopPayment",
                type: "nvarchar(30)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "WorkshopPayment",
                type: "nvarchar(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "CustomerPhone",
                table: "WorkshopPayment",
                type: "nvarchar(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "WorkshopPayment",
                type: "nvarchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.AlterColumn<string>(
                name: "PartName",
                table: "WarrantyClaimPart",
                type: "nvarchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.AlterColumn<string>(
                name: "PartCode",
                table: "WarrantyClaimPart",
                type: "nvarchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.AlterColumn<string>(
                name: "ServiceCenterName",
                table: "WarrantyClaim",
                type: "nvarchar(200)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "MediaUrls",
                table: "WarrantyClaim",
                type: "nvarchar(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "ManufacturerDecision",
                table: "WarrantyClaim",
                type: "nvarchar(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "ManufacturerClaimNumber",
                table: "WarrantyClaim",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "IssueDescription",
                table: "WarrantyClaim",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.AlterColumn<string>(
                name: "ClaimNumber",
                table: "WarrantyClaim",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.AlterColumn<string>(
                name: "PartsJson",
                table: "MaintenanceHistory",
                type: "nvarchar(MAX)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "MaintenanceNumber",
                table: "MaintenanceHistory",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MaintenanceHistory",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
            migrationBuilder.CreateTable(
                name: "PlateDossier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    OutputId = table.Column<int>(type: "int", nullable: true),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DossierNumber = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    RegistrationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ServiceFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    VinNumber = table.Column<string>(type: "nvarchar(50)", nullable: false)
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
                name: "RepairOrder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    TechnicianId = table.Column<int>(type: "int", nullable: true),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    CompletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    ExpectedCompletionTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Mileage = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    PartsCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
                name: "RepairOrderDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    RepairOrderId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: true),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
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
            migrationBuilder.CreateIndex(name: "IX_PlateDossier_OutputId", table: "PlateDossier", column: "OutputId");
            migrationBuilder.CreateIndex(
                name: "IX_RepairOrder_TechnicianId",
                table: "RepairOrder",
                column: "TechnicianId");
            migrationBuilder.CreateIndex(name: "IX_RepairOrder_VehicleId", table: "RepairOrder", column: "VehicleId");
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
            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceHistory_EmployeeProfile_TechnicianId",
                table: "MaintenanceHistory",
                column: "TechnicianId",
                principalTable: "EmployeeProfile",
                principalColumn: "Id");
        }
    }
}
