IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Banner] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(255) NOT NULL,
    [ImageUrl] nvarchar(500) NOT NULL,
    [LinkUrl] nvarchar(500) NULL,
    [CtaText] nvarchar(100) NULL,
    [Placement] nvarchar(50) NULL,
    [Position] nvarchar(50) NULL,
    [StartDate] datetimeoffset NULL,
    [EndDate] datetimeoffset NULL,
    [IsActive] bit NOT NULL,
    [Priority] int NOT NULL,
    [ClickCount] int NOT NULL,
    [ViewCount] int NOT NULL,
    [DisplayOrder] int NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Banner] PRIMARY KEY ([Id])
);

CREATE TABLE [Brand] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NULL,
    [Origin] nvarchar(100) NULL,
    [LogoUrl] nvarchar(1000) NULL,
    [Description] nvarchar(MAX) NULL,
    [RowVersion] rowversion NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Brand] PRIMARY KEY ([Id])
);

CREATE TABLE [Contact] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [Subject] nvarchar(200) NOT NULL,
    [Message] nvarchar(MAX) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [InternalNote] nvarchar(MAX) NULL,
    [Rating] int NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Contact] PRIMARY KEY ([Id])
);

CREATE TABLE [InputStatus] (
    [Key] nvarchar(450) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_InputStatus] PRIMARY KEY ([Key])
);

CREATE TABLE [MediaFiles] (
    [Id] int NOT NULL IDENTITY,
    [StorageType] nvarchar(50) NOT NULL,
    [StoragePath] nvarchar(500) NULL,
    [OriginalFileName] nvarchar(255) NULL,
    [ContentType] nvarchar(100) NULL,
    [FileExtension] nvarchar(100) NULL,
    [FileSize] bigint NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_MediaFiles] PRIMARY KEY ([Id])
);

CREATE TABLE [NewsCategory] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(255) NOT NULL,
    [Slug] varchar(255) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_NewsCategory] PRIMARY KEY ([Id])
);

CREATE TABLE [OutputStatus] (
    [Key] nvarchar(450) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_OutputStatus] PRIMARY KEY ([Key])
);

CREATE TABLE [Permissions] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
);

CREATE TABLE [PredefinedOption] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(100) NOT NULL,
    [Value] nvarchar(200) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_PredefinedOption] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_PredefinedOption_Key] UNIQUE ([Key])
);

CREATE TABLE [ProductCategory] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Slug] nvarchar(max) NULL,
    [ImageUrl] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [SortOrder] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [CategoryGroup] nvarchar(max) NULL,
    [ParentId] int NULL,
    [MaxPurchaseQuantity] int NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ProductCategory] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductCategory_ProductCategory_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [ProductCategory] ([Id])
);

CREATE TABLE [ProductStatus] (
    [Key] nvarchar(450) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ProductStatus] PRIMARY KEY ([Key])
);

CREATE TABLE [Roles] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(max) NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);

CREATE TABLE [Setting] (
    [Key] nvarchar(450) NOT NULL,
    [Value] nvarchar(MAX) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Setting] PRIMARY KEY ([Key])
);

CREATE TABLE [SupplierStatus] (
    [Key] nvarchar(450) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupplierStatus] PRIMARY KEY ([Key])
);

CREATE TABLE [TechnologyCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(255) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_TechnologyCategories] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Gender] nvarchar(max) NOT NULL,
    [RefreshToken] nvarchar(max) NULL,
    [RefreshTokenExpiryTime] datetimeoffset NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NULL,
    [AvatarUrl] nvarchar(max) NULL,
    [DateOfBirth] datetime2 NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [VehicleType] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [Slug] nvarchar(max) NULL,
    [ImageUrl] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [SortOrder] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_VehicleType] PRIMARY KEY ([Id])
);

CREATE TABLE [BannerAuditLog] (
    [Id] int NOT NULL IDENTITY,
    [BannerId] int NOT NULL,
    [Action] nvarchar(50) NOT NULL,
    [ChangedBy] nvarchar(255) NOT NULL,
    [Details] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_BannerAuditLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BannerAuditLog_Banner_BannerId] FOREIGN KEY ([BannerId]) REFERENCES [Banner] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Option] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Option] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Option_PredefinedOption_Name] FOREIGN KEY ([Name]) REFERENCES [PredefinedOption] ([Key]) ON DELETE NO ACTION
);

CREATE TABLE [RoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_RoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RoleClaims_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RolePermissions] (
    [RoleId] uniqueidentifier NOT NULL,
    [PermissionId] int NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
    CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Supplier] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NULL,
    [Phone] nvarchar(15) NULL,
    [Email] nvarchar(50) NULL,
    [StatusId] nvarchar(450) NULL,
    [Notes] nvarchar(MAX) NULL,
    [Address] nvarchar(255) NULL,
    [TaxIdentificationNumber] varchar(20) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Supplier] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Supplier_SupplierStatus_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [SupplierStatus] ([Key])
);

CREATE TABLE [Technologies] (
    [Id] int NOT NULL IDENTITY,
    [CategoryId] int NULL,
    [BrandId] int NULL,
    [Name] nvarchar(255) NOT NULL,
    [DefaultTitle] nvarchar(255) NULL,
    [DefaultDescription] nvarchar(MAX) NULL,
    [DefaultImageUrl] nvarchar(1000) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Technologies] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Technologies_Brand_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brand] ([Id]),
    CONSTRAINT [FK_Technologies_TechnologyCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [TechnologyCategories] ([Id])
);

CREATE TABLE [ContactReply] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [Message] nvarchar(MAX) NOT NULL,
    [RepliedById] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ContactReply] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactReply_Contact_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contact] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ContactReply_Users_RepliedById] FOREIGN KEY ([RepliedById]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [EmployeeProfile] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [IdentityNumber] nvarchar(20) NOT NULL,
    [Address] nvarchar(255) NOT NULL,
    [ContractDate] datetime2 NOT NULL,
    [BankName] nvarchar(100) NOT NULL,
    [BankAccountNumber] nvarchar(50) NOT NULL,
    [JobTitle] nvarchar(50) NOT NULL,
    [BaseSalary] decimal(18,2) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_EmployeeProfile] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmployeeProfile_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Lead] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [Score] int NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Source] nvarchar(50) NOT NULL,
    [InterestedVehicle] nvarchar(255) NOT NULL,
    [Address] nvarchar(500) NOT NULL,
    [AddressDetail] nvarchar(500) NOT NULL,
    [Ward] nvarchar(100) NOT NULL,
    [District] nvarchar(100) NOT NULL,
    [Province] nvarchar(100) NOT NULL,
    [Gender] nvarchar(20) NOT NULL,
    [Birthday] datetime2 NULL,
    [IdentificationNumber] nvarchar(20) NOT NULL,
    [Tier] nvarchar(50) NOT NULL,
    [Points] int NOT NULL,
    [AssignedToId] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Lead] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Lead_Users_AssignedToId] FOREIGN KEY ([AssignedToId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [News] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(255) NOT NULL,
    [Slug] varchar(255) NOT NULL,
    [Content] nvarchar(max) NULL,
    [CoverImageUrl] nvarchar(500) NULL,
    [AuthorName] nvarchar(100) NULL,
    [PublishedDate] datetimeoffset NULL,
    [IsPublished] bit NOT NULL,
    [MetaTitle] nvarchar(100) NULL,
    [MetaDescription] nvarchar(255) NULL,
    [MetaKeywords] nvarchar(255) NULL,
    [CategoryId] int NULL,
    [AuthorId] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_News] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_News_NewsCategory_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [NewsCategory] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_News_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Output] (
    [id] int NOT NULL IDENTITY,
    [CustomerName] nvarchar(max) NULL,
    [CustomerAddress] nvarchar(max) NULL,
    [CustomerPhone] nvarchar(max) NULL,
    [LastStatusChangedAt] datetimeoffset NULL,
    [BuyerId] uniqueidentifier NULL,
    [CreatedBy] uniqueidentifier NULL,
    [FinishedBy] uniqueidentifier NULL,
    [StatusId] nvarchar(450) NULL,
    [PaymentMethod] nvarchar(max) NULL,
    [TransactionId] nvarchar(max) NULL,
    [PaymentStatus] nvarchar(max) NULL,
    [PaidAmount] decimal(18,2) NULL,
    [PaidAt] datetimeoffset NULL,
    [Notes] nvarchar(MAX) NULL,
    [DepositRatio] int NULL,
    [PaymentUrl] nvarchar(MAX) NULL,
    [PaymentCode] nvarchar(max) NULL,
    [PaymentExpiredAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Output] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Output_OutputStatus_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [OutputStatus] ([Key]),
    CONSTRAINT [FK_Output_Users_BuyerId] FOREIGN KEY ([BuyerId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_Output_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_Output_Users_FinishedBy] FOREIGN KEY ([FinishedBy]) REFERENCES [Users] ([Id])
);

CREATE TABLE [UserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_UserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [UserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_UserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_UserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [UserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [ApplicationUserId] uniqueidentifier NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [UserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_UserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Product] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NULL,
    [ShortDescription] nvarchar(255) NULL,
    [MetaTitle] nvarchar(100) NULL,
    [MetaDescription] nvarchar(255) NULL,
    [CategoryId] int NULL,
    [VehicleTypeId] int NULL,
    [StatusId] nvarchar(450) NULL,
    [BrandId] int NULL,
    [Weight] nvarchar(20) NULL,
    [Dimensions] nvarchar(35) NULL,
    [Wheelbase] nvarchar(20) NULL,
    [SeatHeight] nvarchar(20) NULL,
    [GroundClearance] nvarchar(20) NULL,
    [FuelCapacity] nvarchar(20) NULL,
    [TireSize] nvarchar(100) NULL,
    [FrontSuspension] nvarchar(255) NULL,
    [RearSuspension] nvarchar(255) NULL,
    [EngineType] nvarchar(100) NULL,
    [MaxPower] nvarchar(50) NULL,
    [OilCapacity] nvarchar(250) NULL,
    [FuelConsumption] nvarchar(35) NULL,
    [TransmissionType] nvarchar(100) NULL,
    [StarterSystem] nvarchar(30) NULL,
    [MaxTorque] nvarchar(50) NULL,
    [Displacement] nvarchar(50) NULL,
    [BoreStroke] nvarchar(30) NULL,
    [CompressionRatio] nvarchar(10) NULL,
    [FuelSystem] nvarchar(100) NULL,
    [FrameType] nvarchar(100) NULL,
    [FrontTireSize] nvarchar(100) NULL,
    [RearTireSize] nvarchar(100) NULL,
    [FrontBrake] nvarchar(100) NULL,
    [RearBrake] nvarchar(100) NULL,
    [BatteryType] nvarchar(100) NULL,
    [LightingSystem] nvarchar(100) NULL,
    [DashboardType] nvarchar(100) NULL,
    [Material] nvarchar(100) NULL,
    [Origin] nvarchar(100) NULL,
    [WarrantyPeriod] nvarchar(50) NULL,
    [Unit] nvarchar(20) NULL,
    [StdDot] bit NOT NULL,
    [StdEce] bit NOT NULL,
    [StdSnell] bit NOT NULL,
    [StdJis] bit NOT NULL,
    [OtherStandards] nvarchar(255) NULL,
    [Description] nvarchar(MAX) NULL,
    [Highlights] nvarchar(MAX) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Product] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Product_Brand_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brand] ([Id]),
    CONSTRAINT [FK_Product_ProductCategory_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ProductCategory] ([Id]),
    CONSTRAINT [FK_Product_ProductStatus_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [ProductStatus] ([Key]),
    CONSTRAINT [FK_Product_VehicleType_VehicleTypeId] FOREIGN KEY ([VehicleTypeId]) REFERENCES [VehicleType] ([Id])
);

CREATE TABLE [OptionValue] (
    [Id] int NOT NULL IDENTITY,
    [OptionId] int NULL,
    [Name] nvarchar(100) NULL,
    [Description] nvarchar(MAX) NULL,
    [ImageUrl] nvarchar(max) NULL,
    [SeoTitle] nvarchar(200) NULL,
    [SeoDescription] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [ColorCode] nvarchar(20) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_OptionValue] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OptionValue_Option_OptionId] FOREIGN KEY ([OptionId]) REFERENCES [Option] ([Id])
);

CREATE TABLE [SupplierContact] (
    [Id] int NOT NULL IDENTITY,
    [SupplierId] int NULL,
    [Name] nvarchar(100) NULL,
    [Phone] nvarchar(15) NULL,
    [Email] nvarchar(50) NULL,
    [CitizenID] varchar(20) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupplierContact] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupplierContact_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id])
);

CREATE TABLE [TechnologyImages] (
    [Id] int NOT NULL IDENTITY,
    [TechnologyId] int NOT NULL,
    [ImageUrl] nvarchar(1000) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_TechnologyImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TechnologyImages_Technologies_TechnologyId] FOREIGN KEY ([TechnologyId]) REFERENCES [Technologies] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [KPI] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeProfileId] int NOT NULL,
    [MetricName] nvarchar(100) NOT NULL,
    [TargetValue] decimal(18,2) NOT NULL,
    [ActualValue] decimal(18,2) NOT NULL,
    [PeriodStart] datetime2 NOT NULL,
    [PeriodEnd] datetime2 NOT NULL,
    [Description] nvarchar(MAX) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_KPI] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_KPI_EmployeeProfile_EmployeeProfileId] FOREIGN KEY ([EmployeeProfileId]) REFERENCES [EmployeeProfile] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Payroll] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeProfileId] int NOT NULL,
    [Month] int NOT NULL,
    [Year] int NOT NULL,
    [BaseSalary] decimal(18,2) NOT NULL,
    [TotalCommission] decimal(18,2) NOT NULL,
    [Bonus] decimal(18,2) NOT NULL,
    [Penalty] decimal(18,2) NOT NULL,
    [TotalSalary] decimal(18,2) NOT NULL,
    [IsApproved] bit NOT NULL,
    [ApprovedAt] datetime2 NULL,
    [ApprovedBy] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Payroll] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payroll_EmployeeProfile_EmployeeProfileId] FOREIGN KEY ([EmployeeProfileId]) REFERENCES [EmployeeProfile] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [LeadActivity] (
    [Id] int NOT NULL IDENTITY,
    [LeadId] int NOT NULL,
    [ActivityType] nvarchar(50) NOT NULL,
    [Description] nvarchar(MAX) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_LeadActivity] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeadActivity_Lead_LeadId] FOREIGN KEY ([LeadId]) REFERENCES [Lead] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CommissionRecord] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeProfileId] int NOT NULL,
    [OutputId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [DateEarned] datetime2 NOT NULL,
    [PaidAt] datetime2 NULL,
    [PolicySnapshot] nvarchar(MAX) NULL,
    [Note] nvarchar(255) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_CommissionRecord] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CommissionRecord_EmployeeProfile_EmployeeProfileId] FOREIGN KEY ([EmployeeProfileId]) REFERENCES [EmployeeProfile] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CommissionRecord_Output_OutputId] FOREIGN KEY ([OutputId]) REFERENCES [Output] ([id]) ON DELETE CASCADE
);

CREATE TABLE [Input] (
    [Id] int NOT NULL IDENTITY,
    [InputDate] datetimeoffset NULL,
    [Notes] nvarchar(MAX) NULL,
    [StatusId] nvarchar(450) NULL,
    [SupplierId] int NULL,
    [CreatedBy] uniqueidentifier NULL,
    [ConfirmedBy] uniqueidentifier NULL,
    [SourceOrderId] int NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Input] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Input_InputStatus_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [InputStatus] ([Key]),
    CONSTRAINT [FK_Input_Output_SourceOrderId] FOREIGN KEY ([SourceOrderId]) REFERENCES [Output] ([id]),
    CONSTRAINT [FK_Input_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id]),
    CONSTRAINT [FK_Input_Users_ConfirmedBy] FOREIGN KEY ([ConfirmedBy]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_Input_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id])
);

CREATE TABLE [CommissionPolicy] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(500) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Value] decimal(18,2) NOT NULL,
    [ProductId] int NULL,
    [CategoryId] int NULL,
    [EmployeeId] uniqueidentifier NULL,
    [TargetGroup] nvarchar(50) NULL,
    [EffectiveDate] datetimeoffset NOT NULL,
    [Notes] nvarchar(500) NULL,
    [Unit] nvarchar(20) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_CommissionPolicy] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CommissionPolicy_ProductCategory_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ProductCategory] ([Id]),
    CONSTRAINT [FK_CommissionPolicy_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Product] ([Id])
);

CREATE TABLE [ProductCompatibility] (
    [Id] int NOT NULL IDENTITY,
    [BaseProductId] int NOT NULL,
    [CompatibleVehicleModelId] int NOT NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ProductCompatibility] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductCompatibility_Product_BaseProductId] FOREIGN KEY ([BaseProductId]) REFERENCES [Product] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductCompatibility_Product_CompatibleVehicleModelId] FOREIGN KEY ([CompatibleVehicleModelId]) REFERENCES [Product] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ProductTechnology] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [TechnologyId] int NOT NULL,
    [DisplayOrder] int NOT NULL,
    [CustomTitle] nvarchar(255) NULL,
    [CustomDescription] nvarchar(MAX) NULL,
    [CustomImageUrl] nvarchar(1000) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ProductTechnology] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductTechnology_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Product] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductTechnology_Technologies_TechnologyId] FOREIGN KEY ([TechnologyId]) REFERENCES [Technologies] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProductVariant] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [UrlSlug] nvarchar(255) NULL,
    [Price] decimal(18,2) NULL,
    [CoverImageUrl] nvarchar(1000) NULL,
    [VersionName] nvarchar(100) NULL,
    [ColorName] nvarchar(500) NULL,
    [ColorCode] nvarchar(200) NULL,
    [SKU] nvarchar(50) NULL,
    [Weight] decimal(18,2) NULL,
    [Dimensions] nvarchar(35) NULL,
    [Wheelbase] decimal(18,2) NULL,
    [SeatHeight] decimal(18,2) NULL,
    [GroundClearance] decimal(18,2) NULL,
    [FuelCapacity] decimal(18,2) NULL,
    [TireSize] nvarchar(100) NULL,
    [FrontBrake] nvarchar(100) NULL,
    [RearBrake] nvarchar(100) NULL,
    [FrontSuspension] nvarchar(255) NULL,
    [RearSuspension] nvarchar(255) NULL,
    [EngineType] nvarchar(100) NULL,
    [StockQuantity] int NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ProductVariant] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductVariant_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Product] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Vehicle] (
    [Id] int NOT NULL IDENTITY,
    [LeadId] int NOT NULL,
    [ProductId] int NULL,
    [VinNumber] nvarchar(100) NOT NULL,
    [EngineNumber] nvarchar(100) NOT NULL,
    [LicensePlate] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL,
    [PurchaseDate] datetimeoffset NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Vehicle] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Vehicle_Lead_LeadId] FOREIGN KEY ([LeadId]) REFERENCES [Lead] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Vehicle_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Product] ([Id])
);

CREATE TABLE [CommissionPolicyAuditLog] (
    [Id] int NOT NULL IDENTITY,
    [PolicyId] int NOT NULL,
    [Action] nvarchar(20) NOT NULL,
    [ChangedByName] nvarchar(200) NOT NULL,
    [ChangedByUserId] uniqueidentifier NOT NULL,
    [OldValueSnapshot] nvarchar(MAX) NULL,
    [NewValueSnapshot] nvarchar(MAX) NULL,
    [Description] nvarchar(500) NULL,
    [ChangedAt] datetime2 NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_CommissionPolicyAuditLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CommissionPolicyAuditLog_CommissionPolicy_PolicyId] FOREIGN KEY ([PolicyId]) REFERENCES [CommissionPolicy] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Booking] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [ProductVariantId] int NULL,
    [PreferredDate] datetimeoffset NOT NULL,
    [Note] nvarchar(MAX) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [BookingType] nvarchar(20) NOT NULL,
    [Location] nvarchar(200) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Booking] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Booking_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id])
);

CREATE TABLE [OutputInfo] (
    [id] int NOT NULL IDENTITY,
    [ProductVarientId] int NULL,
    [Count] int NULL,
    [OutputId] int NOT NULL,
    [Price] decimal(18,2) NULL,
    [CostPrice] decimal(18,2) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_OutputInfo] PRIMARY KEY ([id]),
    CONSTRAINT [FK_OutputInfo_Output_OutputId] FOREIGN KEY ([OutputId]) REFERENCES [Output] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OutputInfo_ProductVariant_ProductVarientId] FOREIGN KEY ([ProductVarientId]) REFERENCES [ProductVariant] ([Id])
);

CREATE TABLE [ProductCollectionPhoto] (
    [Id] int NOT NULL IDENTITY,
    [ProductVariantId] int NOT NULL,
    [ImageUrl] nvarchar(100) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ProductCollectionPhoto] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductCollectionPhoto_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [VariantOptionValue] (
    [Id] int NOT NULL IDENTITY,
    [VariantId] int NOT NULL,
    [OptionValueId] int NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_VariantOptionValue] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VariantOptionValue_OptionValue_OptionValueId] FOREIGN KEY ([OptionValueId]) REFERENCES [OptionValue] ([Id]),
    CONSTRAINT [FK_VariantOptionValue_ProductVariant_VariantId] FOREIGN KEY ([VariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [MaintenanceHistory] (
    [Id] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [MaintenanceDate] datetimeoffset NOT NULL,
    [Mileage] int NOT NULL,
    [Description] nvarchar(MAX) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_MaintenanceHistory] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MaintenanceHistory_Vehicle_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicle] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [VehicleDocument] (
    [Id] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [DocumentType] nvarchar(50) NOT NULL,
    [FileUrl] nvarchar(500) NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_VehicleDocument] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VehicleDocument_Vehicle_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicle] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [InputInfo] (
    [Id] int NOT NULL IDENTITY,
    [InputId] int NOT NULL,
    [ProductId] int NULL,
    [Count] int NULL,
    [InputPrice] decimal(18,2) NULL,
    [RemainingCount] int NULL,
    [ParentOutputInfoId] int NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_InputInfo] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InputInfo_Input_InputId] FOREIGN KEY ([InputId]) REFERENCES [Input] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InputInfo_OutputInfo_ParentOutputInfoId] FOREIGN KEY ([ParentOutputInfoId]) REFERENCES [OutputInfo] ([id]),
    CONSTRAINT [FK_InputInfo_ProductVariant_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [ProductVariant] ([Id])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Key', N'CreatedAt', N'DeletedAt', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[ProductStatus]'))
    SET IDENTITY_INSERT [ProductStatus] ON;
INSERT INTO [ProductStatus] ([Key], [CreatedAt], [DeletedAt], [UpdatedAt])
VALUES (N'for-sale', NULL, NULL, NULL),
(N'out-of-business', NULL, NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Key', N'CreatedAt', N'DeletedAt', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[ProductStatus]'))
    SET IDENTITY_INSERT [ProductStatus] OFF;

CREATE INDEX [IX_BannerAuditLog_BannerId] ON [BannerAuditLog] ([BannerId]);

CREATE INDEX [IX_Booking_ProductVariantId] ON [Booking] ([ProductVariantId]);

CREATE INDEX [IX_CommissionPolicy_CategoryId] ON [CommissionPolicy] ([CategoryId]);

CREATE INDEX [IX_CommissionPolicy_ProductId] ON [CommissionPolicy] ([ProductId]);

CREATE INDEX [IX_CommissionPolicyAuditLog_PolicyId] ON [CommissionPolicyAuditLog] ([PolicyId]);

CREATE INDEX [IX_CommissionRecord_EmployeeProfileId] ON [CommissionRecord] ([EmployeeProfileId]);

CREATE INDEX [IX_CommissionRecord_OutputId] ON [CommissionRecord] ([OutputId]);

CREATE INDEX [IX_ContactReply_ContactId] ON [ContactReply] ([ContactId]);

CREATE INDEX [IX_ContactReply_RepliedById] ON [ContactReply] ([RepliedById]);

CREATE INDEX [IX_EmployeeProfile_UserId] ON [EmployeeProfile] ([UserId]);

CREATE INDEX [IX_Input_ConfirmedBy] ON [Input] ([ConfirmedBy]);

CREATE INDEX [IX_Input_CreatedBy] ON [Input] ([CreatedBy]);

CREATE INDEX [IX_Input_SourceOrderId] ON [Input] ([SourceOrderId]);

CREATE INDEX [IX_Input_StatusId] ON [Input] ([StatusId]);

CREATE INDEX [IX_Input_SupplierId] ON [Input] ([SupplierId]);

CREATE INDEX [IX_InputInfo_InputId] ON [InputInfo] ([InputId]);

CREATE INDEX [IX_InputInfo_ParentOutputInfoId] ON [InputInfo] ([ParentOutputInfoId]);

CREATE INDEX [IX_InputInfo_ProductId] ON [InputInfo] ([ProductId]);

CREATE INDEX [IX_KPI_EmployeeProfileId] ON [KPI] ([EmployeeProfileId]);

CREATE INDEX [IX_Lead_AssignedToId] ON [Lead] ([AssignedToId]);

CREATE INDEX [IX_LeadActivity_LeadId] ON [LeadActivity] ([LeadId]);

CREATE INDEX [IX_MaintenanceHistory_VehicleId] ON [MaintenanceHistory] ([VehicleId]);

CREATE INDEX [IX_News_AuthorId] ON [News] ([AuthorId]);

CREATE INDEX [IX_News_CategoryId] ON [News] ([CategoryId]);

CREATE INDEX [IX_Option_Name] ON [Option] ([Name]);

CREATE INDEX [IX_OptionValue_OptionId] ON [OptionValue] ([OptionId]);

CREATE INDEX [IX_Output_BuyerId] ON [Output] ([BuyerId]);

CREATE INDEX [IX_Output_CreatedBy] ON [Output] ([CreatedBy]);

CREATE INDEX [IX_Output_FinishedBy] ON [Output] ([FinishedBy]);

CREATE INDEX [IX_Output_StatusId] ON [Output] ([StatusId]);

CREATE INDEX [IX_OutputInfo_OutputId] ON [OutputInfo] ([OutputId]);

CREATE INDEX [IX_OutputInfo_ProductVarientId] ON [OutputInfo] ([ProductVarientId]);

CREATE INDEX [IX_Payroll_EmployeeProfileId] ON [Payroll] ([EmployeeProfileId]);

CREATE UNIQUE INDEX [IX_PredefinedOption_Key] ON [PredefinedOption] ([Key]);

CREATE INDEX [IX_Product_BrandId] ON [Product] ([BrandId]);

CREATE INDEX [IX_Product_CategoryId] ON [Product] ([CategoryId]);

CREATE INDEX [IX_Product_StatusId] ON [Product] ([StatusId]);

CREATE INDEX [IX_Product_VehicleTypeId] ON [Product] ([VehicleTypeId]);

CREATE INDEX [IX_ProductCategory_ParentId] ON [ProductCategory] ([ParentId]);

CREATE INDEX [IX_ProductCollectionPhoto_ProductVariantId] ON [ProductCollectionPhoto] ([ProductVariantId]);

CREATE INDEX [IX_ProductCompatibility_BaseProductId] ON [ProductCompatibility] ([BaseProductId]);

CREATE INDEX [IX_ProductCompatibility_CompatibleVehicleModelId] ON [ProductCompatibility] ([CompatibleVehicleModelId]);

CREATE INDEX [IX_ProductTechnology_ProductId] ON [ProductTechnology] ([ProductId]);

CREATE INDEX [IX_ProductTechnology_TechnologyId] ON [ProductTechnology] ([TechnologyId]);

CREATE INDEX [IX_ProductVariant_ProductId] ON [ProductVariant] ([ProductId]);

CREATE INDEX [IX_RoleClaims_RoleId] ON [RoleClaims] ([RoleId]);

CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_Supplier_StatusId] ON [Supplier] ([StatusId]);

CREATE INDEX [IX_SupplierContact_SupplierId] ON [SupplierContact] ([SupplierId]);

CREATE INDEX [IX_Technologies_BrandId] ON [Technologies] ([BrandId]);

CREATE INDEX [IX_Technologies_CategoryId] ON [Technologies] ([CategoryId]);

CREATE INDEX [IX_TechnologyImages_TechnologyId] ON [TechnologyImages] ([TechnologyId]);

CREATE INDEX [IX_UserClaims_UserId] ON [UserClaims] ([UserId]);

CREATE INDEX [IX_UserLogins_UserId] ON [UserLogins] ([UserId]);

CREATE INDEX [IX_UserRoles_ApplicationUserId] ON [UserRoles] ([ApplicationUserId]);

CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [Users] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_VariantOptionValue_OptionValueId] ON [VariantOptionValue] ([OptionValueId]);

CREATE INDEX [IX_VariantOptionValue_VariantId] ON [VariantOptionValue] ([VariantId]);

CREATE INDEX [IX_Vehicle_LeadId] ON [Vehicle] ([LeadId]);

CREATE INDEX [IX_Vehicle_ProductId] ON [Vehicle] ([ProductId]);

CREATE INDEX [IX_VehicleDocument_VehicleId] ON [VehicleDocument] ([VehicleId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260509132251_InitialCreate', N'10.0.7');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Supplier] ADD [PartnerTypeId] nvarchar(50) NULL;

CREATE TABLE [PartnerType] (
    [Key] nvarchar(50) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_PartnerType] PRIMARY KEY ([Key])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Key', N'CreatedAt', N'DeletedAt', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[PartnerType]'))
    SET IDENTITY_INSERT [PartnerType] ON;
INSERT INTO [PartnerType] ([Key], [CreatedAt], [DeletedAt], [UpdatedAt])
VALUES (N'financial', NULL, NULL, NULL),
(N'insurance', NULL, NULL, NULL),
(N'supplier', NULL, NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Key', N'CreatedAt', N'DeletedAt', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[PartnerType]'))
    SET IDENTITY_INSERT [PartnerType] OFF;

CREATE INDEX [IX_Supplier_PartnerTypeId] ON [Supplier] ([PartnerTypeId]);

ALTER TABLE [Supplier] ADD CONSTRAINT [FK_Supplier_PartnerType_PartnerTypeId] FOREIGN KEY ([PartnerTypeId]) REFERENCES [PartnerType] ([Key]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260516011310_AddSupplierTypeIdColumn', N'10.0.7');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Product] DROP CONSTRAINT [FK_Product_VehicleType_VehicleTypeId];

DROP TABLE [VehicleType];

DROP INDEX [IX_Product_VehicleTypeId] ON [Product];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductVariant]') AND [c].[name] = N'StockQuantity');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [ProductVariant] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [ProductVariant] DROP COLUMN [StockQuantity];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductCategory]') AND [c].[name] = N'SortOrder');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ProductCategory] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [ProductCategory] DROP COLUMN [SortOrder];

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Product]') AND [c].[name] = N'VehicleTypeId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Product] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Product] DROP COLUMN [VehicleTypeId];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260519141635_DropVehicleTypeAndUnusedProductColumns', N'10.0.7');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Vehicle] DROP CONSTRAINT [FK_Vehicle_Lead_LeadId];

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductVariant]') AND [c].[name] = N'ColorCode');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ProductVariant] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [ProductVariant] DROP COLUMN [ColorCode];

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductVariant]') AND [c].[name] = N'ColorName');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [ProductVariant] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [ProductVariant] DROP COLUMN [ColorName];

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductCategory]') AND [c].[name] = N'CategoryGroup');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [ProductCategory] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [ProductCategory] DROP COLUMN [CategoryGroup];

EXEC sp_rename N'[ProductVariant].[VersionName]', N'VariantName', 'COLUMN';

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vehicle]') AND [c].[name] = N'LeadId');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Vehicle] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [Vehicle] ALTER COLUMN [LeadId] int NULL;

ALTER TABLE [Vehicle] ADD [InputInfoId] int NULL;

ALTER TABLE [Vehicle] ADD [OutputInfoId] int NULL;

ALTER TABLE [ProductCategory] ADD [ManagementType] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [OutputInfo] ADD [ProductVariantColorId] int NULL;

ALTER TABLE [InputInfo] ADD [ProductVariantColorId] int NULL;

CREATE TABLE [ProductVariantColor] (
    [Id] int NOT NULL IDENTITY,
    [ProductVariantId] int NOT NULL,
    [ColorName] nvarchar(500) NULL,
    [ColorCode] nvarchar(200) NULL,
    [CoverImageUrl] nvarchar(1000) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ProductVariantColor] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductVariantColor_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Vehicle_InputInfoId] ON [Vehicle] ([InputInfoId]);

CREATE INDEX [IX_Vehicle_OutputInfoId] ON [Vehicle] ([OutputInfoId]);

CREATE INDEX [IX_OutputInfo_ProductVariantColorId] ON [OutputInfo] ([ProductVariantColorId]);

CREATE INDEX [IX_InputInfo_ProductVariantColorId] ON [InputInfo] ([ProductVariantColorId]);

CREATE INDEX [IX_ProductVariantColor_ProductVariantId] ON [ProductVariantColor] ([ProductVariantId]);

ALTER TABLE [InputInfo] ADD CONSTRAINT [FK_InputInfo_ProductVariantColor_ProductVariantColorId] FOREIGN KEY ([ProductVariantColorId]) REFERENCES [ProductVariantColor] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [OutputInfo] ADD CONSTRAINT [FK_OutputInfo_ProductVariantColor_ProductVariantColorId] FOREIGN KEY ([ProductVariantColorId]) REFERENCES [ProductVariantColor] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Vehicle] ADD CONSTRAINT [FK_Vehicle_InputInfo_InputInfoId] FOREIGN KEY ([InputInfoId]) REFERENCES [InputInfo] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Vehicle] ADD CONSTRAINT [FK_Vehicle_Lead_LeadId] FOREIGN KEY ([LeadId]) REFERENCES [Lead] ([Id]);

ALTER TABLE [Vehicle] ADD CONSTRAINT [FK_Vehicle_OutputInfo_OutputInfoId] FOREIGN KEY ([OutputInfoId]) REFERENCES [OutputInfo] ([id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260521085746_AddVehicleTrackingAndColorLinking', N'10.0.7');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [PlateDossier] (
    [Id] int NOT NULL IDENTITY,
    [OutputId] int NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [LicensePlate] nvarchar(50) NOT NULL,
    [RegistrationFee] decimal(18,2) NOT NULL,
    [ActualCost] decimal(18,2) NOT NULL,
    [ServiceFee] decimal(18,2) NOT NULL,
    [Notes] nvarchar(MAX) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_PlateDossier] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlateDossier_Output_OutputId] FOREIGN KEY ([OutputId]) REFERENCES [Output] ([id]) ON DELETE CASCADE
);

CREATE TABLE [RepairOrder] (
    [Id] int NOT NULL IDENTITY,
    [VehicleId] int NULL,
    [CustomerName] nvarchar(100) NOT NULL,
    [CustomerPhone] nvarchar(20) NOT NULL,
    [Mileage] int NOT NULL,
    [Description] nvarchar(MAX) NOT NULL,
    [TechnicianId] int NULL,
    [Status] nvarchar(20) NOT NULL,
    [LaborCost] decimal(18,2) NOT NULL,
    [PartsCost] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [PaymentStatus] nvarchar(20) NOT NULL,
    [PaymentMethod] nvarchar(50) NULL,
    [Notes] nvarchar(MAX) NULL,
    [CompletedDate] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_RepairOrder] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RepairOrder_EmployeeProfile_TechnicianId] FOREIGN KEY ([TechnicianId]) REFERENCES [EmployeeProfile] ([Id]),
    CONSTRAINT [FK_RepairOrder_Vehicle_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicle] ([Id])
);

CREATE TABLE [ServiceCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ServiceCategories] PRIMARY KEY ([Id])
);

CREATE TABLE [Services] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [BasePrice] decimal(18,2) NOT NULL,
    [EstimatedDurationMinutes] int NULL,
    [CategoryId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Services] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Services_ServiceCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ServiceCategories] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RepairOrderDetail] (
    [Id] int NOT NULL IDENTITY,
    [RepairOrderId] int NOT NULL,
    [ServiceId] int NULL,
    [ProductVariantId] int NULL,
    [Count] int NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [LaborCost] decimal(18,2) NOT NULL,
    [Type] nvarchar(20) NOT NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_RepairOrderDetail] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RepairOrderDetail_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]),
    CONSTRAINT [FK_RepairOrderDetail_RepairOrder_RepairOrderId] FOREIGN KEY ([RepairOrderId]) REFERENCES [RepairOrder] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RepairOrderDetail_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id])
);

CREATE TABLE [ServiceBooking] (
    [Id] int NOT NULL IDENTITY,
    [ServiceId] int NOT NULL,
    [VehicleId] int NULL,
    [CustomerId] uniqueidentifier NULL,
    [TechnicianId] int NULL,
    [ScheduledDate] datetimeoffset NOT NULL,
    [EstimatedDurationMinutes] int NULL,
    [Status] nvarchar(20) NOT NULL,
    [PaymentStatus] nvarchar(20) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [DepositAmount] decimal(18,2) NULL,
    [Notes] nvarchar(MAX) NULL,
    [CustomerNotes] nvarchar(MAX) NULL,
    [TechnicianNotes] nvarchar(MAX) NULL,
    [CompletedDate] datetimeoffset NULL,
    [CancelledDate] datetimeoffset NULL,
    [CancelledReason] nvarchar(500) NULL,
    [Rating] int NULL,
    [Review] nvarchar(MAX) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ServiceBooking] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServiceBooking_EmployeeProfile_TechnicianId] FOREIGN KEY ([TechnicianId]) REFERENCES [EmployeeProfile] ([Id]),
    CONSTRAINT [FK_ServiceBooking_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ServiceBooking_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_ServiceBooking_Vehicle_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicle] ([Id])
);

CREATE INDEX [IX_PlateDossier_OutputId] ON [PlateDossier] ([OutputId]);

CREATE INDEX [IX_RepairOrder_TechnicianId] ON [RepairOrder] ([TechnicianId]);

CREATE INDEX [IX_RepairOrder_VehicleId] ON [RepairOrder] ([VehicleId]);

CREATE INDEX [IX_RepairOrderDetail_ProductVariantId] ON [RepairOrderDetail] ([ProductVariantId]);

CREATE INDEX [IX_RepairOrderDetail_RepairOrderId] ON [RepairOrderDetail] ([RepairOrderId]);

CREATE INDEX [IX_RepairOrderDetail_ServiceId] ON [RepairOrderDetail] ([ServiceId]);

CREATE INDEX [IX_ServiceBooking_CustomerId] ON [ServiceBooking] ([CustomerId]);

CREATE INDEX [IX_ServiceBooking_ServiceId] ON [ServiceBooking] ([ServiceId]);

CREATE INDEX [IX_ServiceBooking_TechnicianId] ON [ServiceBooking] ([TechnicianId]);

CREATE INDEX [IX_ServiceBooking_VehicleId] ON [ServiceBooking] ([VehicleId]);

CREATE INDEX [IX_Services_CategoryId] ON [Services] ([CategoryId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260522140635_AddModule6Entities', N'10.0.7');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [RepairOrder] ADD [ExpectedCompletionTime] datetimeoffset NULL;

ALTER TABLE [RepairOrder] ADD [StartTime] datetimeoffset NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260525092651_FixPendingModelChanges', N'10.0.7');

COMMIT;
GO

