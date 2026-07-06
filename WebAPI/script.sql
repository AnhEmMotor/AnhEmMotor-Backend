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
VALUES (N'20260509132251_InitialCreate', N'10.0.9');

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
VALUES (N'20260516011310_AddSupplierTypeIdColumn', N'10.0.9');

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
VALUES (N'20260519141635_DropVehicleTypeAndUnusedProductColumns', N'10.0.9');

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
VALUES (N'20260521085746_AddVehicleTrackingAndColorLinking', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [InputInfo] DROP CONSTRAINT [FK_InputInfo_ProductVariant_ProductId];

ALTER TABLE [OutputInfo] DROP CONSTRAINT [FK_OutputInfo_ProductVariant_ProductVarientId];

EXEC sp_rename N'[OutputInfo].[ProductVarientId]', N'ProductVariantId', 'COLUMN';

EXEC sp_rename N'[OutputInfo].[IX_OutputInfo_ProductVarientId]', N'IX_OutputInfo_ProductVariantId', 'INDEX';

EXEC sp_rename N'[InputInfo].[ProductId]', N'ProductVariantId', 'COLUMN';

EXEC sp_rename N'[InputInfo].[IX_InputInfo_ProductId]', N'IX_InputInfo_ProductVariantId', 'INDEX';

ALTER TABLE [Vehicle] ADD [ProductVariantColorId] int NULL;

ALTER TABLE [Vehicle] ADD [ProductVariantId] int NULL;

ALTER TABLE [Vehicle] ADD [Status] nvarchar(50) NOT NULL DEFAULT N'';

CREATE INDEX [IX_Vehicle_ProductVariantColorId] ON [Vehicle] ([ProductVariantColorId]);

CREATE INDEX [IX_Vehicle_ProductVariantId] ON [Vehicle] ([ProductVariantId]);

ALTER TABLE [InputInfo] ADD CONSTRAINT [FK_InputInfo_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]);

ALTER TABLE [OutputInfo] ADD CONSTRAINT [FK_OutputInfo_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]);

ALTER TABLE [Vehicle] ADD CONSTRAINT [FK_Vehicle_ProductVariantColor_ProductVariantColorId] FOREIGN KEY ([ProductVariantColorId]) REFERENCES [ProductVariantColor] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Vehicle] ADD CONSTRAINT [FK_Vehicle_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260522145111_FixProductVariantNamingAndAddVehicleColumns', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [Quotations] (
    [Id] int NOT NULL IDENTITY,
    [SupplierId] int NULL,
    [Status] varchar(30) NULL,
    [Note] nvarchar(MAX) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Quotations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Quotations_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [QuotationProductRows] (
    [Id] int NOT NULL IDENTITY,
    [QuotationId] int NULL,
    [ProductVariantId] int NULL,
    [ProductVariantColorId] int NULL,
    [QuotePrice] int NULL,
    [Note] nvarchar(MAX) NULL,
    CONSTRAINT [PK_QuotationProductRows] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuotationProductRows_ProductVariantColor_ProductVariantColorId] FOREIGN KEY ([ProductVariantColorId]) REFERENCES [ProductVariantColor] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_QuotationProductRows_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_QuotationProductRows_Quotations_QuotationId] FOREIGN KEY ([QuotationId]) REFERENCES [Quotations] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_QuotationProductRows_ProductVariantColorId] ON [QuotationProductRows] ([ProductVariantColorId]);

CREATE INDEX [IX_QuotationProductRows_ProductVariantId] ON [QuotationProductRows] ([ProductVariantId]);

CREATE INDEX [IX_QuotationProductRows_QuotationId] ON [QuotationProductRows] ([QuotationId]);

CREATE INDEX [IX_Quotations_SupplierId] ON [Quotations] ([SupplierId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260527081022_AddQuotationAndProductRows', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Vehicle] DROP CONSTRAINT [FK_Vehicle_InputInfo_InputInfoId];

DROP TABLE [InputInfo];

DROP TABLE [Input];

DROP TABLE [InputStatus];

DECLARE @var7 nvarchar(max);
SELECT @var7 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Product]') AND [c].[name] = N'Highlights');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Product] DROP CONSTRAINT ' + @var7 + ';');
ALTER TABLE [Product] DROP COLUMN [Highlights];

EXEC sp_rename N'[Vehicle].[InputInfoId]', N'InventoryReceiptInfoId', 'COLUMN';

EXEC sp_rename N'[Vehicle].[IX_Vehicle_InputInfoId]', N'IX_Vehicle_InventoryReceiptInfoId', 'INDEX';

CREATE TABLE [InventoryReceiptStatus] (
    [Key] nvarchar(450) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_InventoryReceiptStatus] PRIMARY KEY ([Key])
);

CREATE TABLE [PurchaseRequest] (
    [Id] int NOT NULL IDENTITY,
    [Status] varchar(30) NOT NULL,
    [Note] nvarchar(MAX) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [ApprovedBy] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_PurchaseRequest] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchaseRequest_Users_ApprovedBy] FOREIGN KEY ([ApprovedBy]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_PurchaseRequest_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id])
);

CREATE TABLE [InventoryReceipt] (
    [Id] int NOT NULL IDENTITY,
    [InventoryReceiptDate] datetimeoffset NULL,
    [Notes] nvarchar(MAX) NULL,
    [StatusId] nvarchar(450) NULL,
    [PurchaseRequestId] int NULL,
    [CreatedBy] uniqueidentifier NULL,
    [ConfirmedBy] uniqueidentifier NULL,
    [SourceOrderId] int NULL,
    [SupplierId] int NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_InventoryReceipt] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryReceipt_InventoryReceiptStatus_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [InventoryReceiptStatus] ([Key]),
    CONSTRAINT [FK_InventoryReceipt_Output_SourceOrderId] FOREIGN KEY ([SourceOrderId]) REFERENCES [Output] ([id]),
    CONSTRAINT [FK_InventoryReceipt_PurchaseRequest_PurchaseRequestId] FOREIGN KEY ([PurchaseRequestId]) REFERENCES [PurchaseRequest] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InventoryReceipt_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id]),
    CONSTRAINT [FK_InventoryReceipt_Users_ConfirmedBy] FOREIGN KEY ([ConfirmedBy]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_InventoryReceipt_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id])
);

CREATE TABLE [PurchaseRequestItem] (
    [Id] int NOT NULL IDENTITY,
    [PurchaseRequestId] int NOT NULL,
    [ProductVariantId] int NOT NULL,
    [ProductVariantColorId] int NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_PurchaseRequestItem] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchaseRequestItem_ProductVariantColor_ProductVariantColorId] FOREIGN KEY ([ProductVariantColorId]) REFERENCES [ProductVariantColor] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PurchaseRequestItem_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PurchaseRequestItem_PurchaseRequest_PurchaseRequestId] FOREIGN KEY ([PurchaseRequestId]) REFERENCES [PurchaseRequest] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [InventoryReceiptInfo] (
    [Id] int NOT NULL IDENTITY,
    [InventoryReceiptId] int NOT NULL,
    [Count] int NULL,
    [RemainingCount] int NULL,
    [ParentOutputInfoId] int NULL,
    [PurchaseRequestItemId] int NULL,
    [QuotationProductRowId] int NULL,
    [ProductVariantId] int NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_InventoryReceiptInfo] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryReceiptInfo_InventoryReceipt_InventoryReceiptId] FOREIGN KEY ([InventoryReceiptId]) REFERENCES [InventoryReceipt] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InventoryReceiptInfo_OutputInfo_ParentOutputInfoId] FOREIGN KEY ([ParentOutputInfoId]) REFERENCES [OutputInfo] ([id]),
    CONSTRAINT [FK_InventoryReceiptInfo_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]),
    CONSTRAINT [FK_InventoryReceiptInfo_PurchaseRequestItem_PurchaseRequestItemId] FOREIGN KEY ([PurchaseRequestItemId]) REFERENCES [PurchaseRequestItem] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InventoryReceiptInfo_QuotationProductRows_QuotationProductRowId] FOREIGN KEY ([QuotationProductRowId]) REFERENCES [QuotationProductRows] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_InventoryReceipt_ConfirmedBy] ON [InventoryReceipt] ([ConfirmedBy]);

CREATE INDEX [IX_InventoryReceipt_CreatedBy] ON [InventoryReceipt] ([CreatedBy]);

CREATE INDEX [IX_InventoryReceipt_PurchaseRequestId] ON [InventoryReceipt] ([PurchaseRequestId]);

CREATE INDEX [IX_InventoryReceipt_SourceOrderId] ON [InventoryReceipt] ([SourceOrderId]);

CREATE INDEX [IX_InventoryReceipt_StatusId] ON [InventoryReceipt] ([StatusId]);

CREATE INDEX [IX_InventoryReceipt_SupplierId] ON [InventoryReceipt] ([SupplierId]);

CREATE INDEX [IX_InventoryReceiptInfo_InventoryReceiptId] ON [InventoryReceiptInfo] ([InventoryReceiptId]);

CREATE INDEX [IX_InventoryReceiptInfo_ParentOutputInfoId] ON [InventoryReceiptInfo] ([ParentOutputInfoId]);

CREATE INDEX [IX_InventoryReceiptInfo_ProductVariantId] ON [InventoryReceiptInfo] ([ProductVariantId]);

CREATE INDEX [IX_InventoryReceiptInfo_PurchaseRequestItemId] ON [InventoryReceiptInfo] ([PurchaseRequestItemId]);

CREATE INDEX [IX_InventoryReceiptInfo_QuotationProductRowId] ON [InventoryReceiptInfo] ([QuotationProductRowId]);

CREATE INDEX [IX_PurchaseRequest_ApprovedBy] ON [PurchaseRequest] ([ApprovedBy]);

CREATE INDEX [IX_PurchaseRequest_CreatedBy] ON [PurchaseRequest] ([CreatedBy]);

CREATE INDEX [IX_PurchaseRequestItem_ProductVariantColorId] ON [PurchaseRequestItem] ([ProductVariantColorId]);

CREATE INDEX [IX_PurchaseRequestItem_ProductVariantId] ON [PurchaseRequestItem] ([ProductVariantId]);

CREATE INDEX [IX_PurchaseRequestItem_PurchaseRequestId] ON [PurchaseRequestItem] ([PurchaseRequestId]);

ALTER TABLE [Vehicle] ADD CONSTRAINT [FK_Vehicle_InventoryReceiptInfo_InventoryReceiptInfoId] FOREIGN KEY ([InventoryReceiptInfoId]) REFERENCES [InventoryReceiptInfo] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260530013138_RefactorInputToInventoryReceiptAndPurchaseRequest', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [InventoryReceipt] DROP CONSTRAINT [FK_InventoryReceipt_Supplier_SupplierId];

ALTER TABLE [InventoryReceiptInfo] DROP CONSTRAINT [FK_InventoryReceiptInfo_QuotationProductRows_QuotationProductRowId];

DROP TABLE [QuotationProductRows];

DROP TABLE [Quotations];

DROP INDEX [IX_InventoryReceipt_SupplierId] ON [InventoryReceipt];

DECLARE @var8 nvarchar(max);
SELECT @var8 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InventoryReceipt]') AND [c].[name] = N'SupplierId');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [InventoryReceipt] DROP CONSTRAINT ' + @var8 + ';');
ALTER TABLE [InventoryReceipt] DROP COLUMN [SupplierId];

DECLARE @var9 nvarchar(max);
SELECT @var9 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Banner]') AND [c].[name] = N'ClickCount');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Banner] DROP CONSTRAINT ' + @var9 + ';');
ALTER TABLE [Banner] DROP COLUMN [ClickCount];

DECLARE @var10 nvarchar(max);
SELECT @var10 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Banner]') AND [c].[name] = N'DisplayOrder');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Banner] DROP CONSTRAINT ' + @var10 + ';');
ALTER TABLE [Banner] DROP COLUMN [DisplayOrder];

DECLARE @var11 nvarchar(max);
SELECT @var11 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Banner]') AND [c].[name] = N'EndDate');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Banner] DROP CONSTRAINT ' + @var11 + ';');
ALTER TABLE [Banner] DROP COLUMN [EndDate];

DECLARE @var12 nvarchar(max);
SELECT @var12 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Banner]') AND [c].[name] = N'IsActive');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Banner] DROP CONSTRAINT ' + @var12 + ';');
ALTER TABLE [Banner] DROP COLUMN [IsActive];

DECLARE @var13 nvarchar(max);
SELECT @var13 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Banner]') AND [c].[name] = N'Position');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Banner] DROP CONSTRAINT ' + @var13 + ';');
ALTER TABLE [Banner] DROP COLUMN [Position];

DECLARE @var14 nvarchar(max);
SELECT @var14 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Banner]') AND [c].[name] = N'Priority');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Banner] DROP CONSTRAINT ' + @var14 + ';');
ALTER TABLE [Banner] DROP COLUMN [Priority];

DECLARE @var15 nvarchar(max);
SELECT @var15 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Banner]') AND [c].[name] = N'StartDate');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Banner] DROP CONSTRAINT ' + @var15 + ';');
ALTER TABLE [Banner] DROP COLUMN [StartDate];

DECLARE @var16 nvarchar(max);
SELECT @var16 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Banner]') AND [c].[name] = N'ViewCount');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Banner] DROP CONSTRAINT ' + @var16 + ';');
ALTER TABLE [Banner] DROP COLUMN [ViewCount];

EXEC sp_rename N'[InventoryReceiptInfo].[QuotationProductRowId]', N'SupplierId', 'COLUMN';

EXEC sp_rename N'[InventoryReceiptInfo].[IX_InventoryReceiptInfo_QuotationProductRowId]', N'IX_InventoryReceiptInfo_SupplierId', 'INDEX';

EXEC sp_rename N'[Banner].[LinkUrl]', N'MobileImageUrl', 'COLUMN';

EXEC sp_rename N'[Banner].[ImageUrl]', N'DesktopImageUrl', 'COLUMN';

EXEC sp_rename N'[Banner].[CtaText]', N'CtaLabel', 'COLUMN';

ALTER TABLE [Vehicle] ADD [ImportPrice] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [PurchaseRequestItem] ADD [CreatedAt] datetimeoffset NULL;

ALTER TABLE [PurchaseRequestItem] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [PurchaseRequestItem] ADD [UpdatedAt] datetimeoffset NULL;

ALTER TABLE [PurchaseRequest] ADD [RejectedBy] uniqueidentifier NULL;

ALTER TABLE [PurchaseRequest] ADD [SentBy] uniqueidentifier NULL;

ALTER TABLE [InventoryReceiptInfo] ADD [UnitPrice] decimal(18,2) NULL;

ALTER TABLE [InventoryReceipt] ADD [ApprovedBy] uniqueidentifier NULL;

ALTER TABLE [InventoryReceipt] ADD [RejectedBy] uniqueidentifier NULL;

ALTER TABLE [InventoryReceipt] ADD [SentBy] uniqueidentifier NULL;

ALTER TABLE [Banner] ADD [CtaLink] nvarchar(500) NULL;

ALTER TABLE [Banner] ADD [Description] nvarchar(1000) NULL;

CREATE TABLE [InventoryLedger] (
    [Id] int NOT NULL IDENTITY,
    [TransactionDate] datetimeoffset NOT NULL,
    [DocumentCode] nvarchar(50) NOT NULL,
    [TransactionType] nvarchar(50) NOT NULL,
    [ProductVariantId] int NOT NULL,
    [ProductVariantColorId] int NULL,
    [PartnerName] nvarchar(255) NULL,
    [ImportQty] int NOT NULL,
    [ExportQty] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [StockAfter] int NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_InventoryLedger] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryLedger_ProductVariantColor_ProductVariantColorId] FOREIGN KEY ([ProductVariantColorId]) REFERENCES [ProductVariantColor] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InventoryLedger_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [InventoryOnHand] (
    [Id] int NOT NULL IDENTITY,
    [ProductVariantId] int NOT NULL,
    [ProductVariantColorId] int NULL,
    [StockQty] int NOT NULL,
    [ImportedQty] int NOT NULL,
    [ExportedQty] int NOT NULL,
    [OrderedQty] int NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_InventoryOnHand] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryOnHand_ProductVariantColor_ProductVariantColorId] FOREIGN KEY ([ProductVariantColorId]) REFERENCES [ProductVariantColor] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InventoryOnHand_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [NewsComments] (
    [Id] int NOT NULL IDENTITY,
    [NewsId] int NOT NULL,
    [UserId] uniqueidentifier NULL,
    [AuthorName] nvarchar(100) NULL,
    [AuthorEmail] nvarchar(100) NULL,
    [Content] nvarchar(max) NOT NULL,
    [IsApproved] bit NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_NewsComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_NewsComments_News_NewsId] FOREIGN KEY ([NewsId]) REFERENCES [News] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_NewsComments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [NewsProduct] (
    [Id] int NOT NULL IDENTITY,
    [NewsId] int NOT NULL,
    [ProductVariantId] int NOT NULL,
    [ProductVariantColorId] int NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_NewsProduct] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_NewsProduct_News_NewsId] FOREIGN KEY ([NewsId]) REFERENCES [News] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_NewsProduct_ProductVariantColor_ProductVariantColorId] FOREIGN KEY ([ProductVariantColorId]) REFERENCES [ProductVariantColor] ([Id]),
    CONSTRAINT [FK_NewsProduct_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProductQuotations] (
    [Id] int NOT NULL IDENTITY,
    [SupplierId] int NULL,
    [ProductVariantId] int NULL,
    [ProductVariantColorId] int NULL,
    [QuotePrice] int NULL,
    [Note] nvarchar(MAX) NULL,
    CONSTRAINT [PK_ProductQuotations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductQuotations_ProductVariantColor_ProductVariantColorId] FOREIGN KEY ([ProductVariantColorId]) REFERENCES [ProductVariantColor] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProductQuotations_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProductQuotations_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id])
);

CREATE TABLE [SupplierDebt] (
    [Id] int NOT NULL IDENTITY,
    [InventoryReceiptId] int NOT NULL,
    [SupplierId] int NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [PaidAmount] decimal(18,2) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupplierDebt] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupplierDebt_InventoryReceipt_InventoryReceiptId] FOREIGN KEY ([InventoryReceiptId]) REFERENCES [InventoryReceipt] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SupplierDebt_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_PurchaseRequest_RejectedBy] ON [PurchaseRequest] ([RejectedBy]);

CREATE INDEX [IX_PurchaseRequest_SentBy] ON [PurchaseRequest] ([SentBy]);

CREATE INDEX [IX_InventoryReceipt_ApprovedBy] ON [InventoryReceipt] ([ApprovedBy]);

CREATE INDEX [IX_InventoryReceipt_RejectedBy] ON [InventoryReceipt] ([RejectedBy]);

CREATE INDEX [IX_InventoryReceipt_SentBy] ON [InventoryReceipt] ([SentBy]);

CREATE INDEX [IX_InventoryLedger_ProductVariantColorId] ON [InventoryLedger] ([ProductVariantColorId]);

CREATE INDEX [IX_InventoryLedger_ProductVariantId] ON [InventoryLedger] ([ProductVariantId]);

CREATE INDEX [IX_InventoryOnHand_ProductVariantColorId] ON [InventoryOnHand] ([ProductVariantColorId]);

CREATE INDEX [IX_InventoryOnHand_ProductVariantId] ON [InventoryOnHand] ([ProductVariantId]);

CREATE INDEX [IX_NewsComments_NewsId] ON [NewsComments] ([NewsId]);

CREATE INDEX [IX_NewsComments_UserId] ON [NewsComments] ([UserId]);

CREATE INDEX [IX_NewsProduct_NewsId] ON [NewsProduct] ([NewsId]);

CREATE INDEX [IX_NewsProduct_ProductVariantColorId] ON [NewsProduct] ([ProductVariantColorId]);

CREATE INDEX [IX_NewsProduct_ProductVariantId] ON [NewsProduct] ([ProductVariantId]);

CREATE INDEX [IX_ProductQuotations_ProductVariantColorId] ON [ProductQuotations] ([ProductVariantColorId]);

CREATE INDEX [IX_ProductQuotations_ProductVariantId] ON [ProductQuotations] ([ProductVariantId]);

CREATE INDEX [IX_ProductQuotations_SupplierId] ON [ProductQuotations] ([SupplierId]);

CREATE INDEX [IX_SupplierDebt_InventoryReceiptId] ON [SupplierDebt] ([InventoryReceiptId]);

CREATE INDEX [IX_SupplierDebt_SupplierId] ON [SupplierDebt] ([SupplierId]);

ALTER TABLE [InventoryReceipt] ADD CONSTRAINT [FK_InventoryReceipt_Users_ApprovedBy] FOREIGN KEY ([ApprovedBy]) REFERENCES [Users] ([Id]);

ALTER TABLE [InventoryReceipt] ADD CONSTRAINT [FK_InventoryReceipt_Users_RejectedBy] FOREIGN KEY ([RejectedBy]) REFERENCES [Users] ([Id]);

ALTER TABLE [InventoryReceipt] ADD CONSTRAINT [FK_InventoryReceipt_Users_SentBy] FOREIGN KEY ([SentBy]) REFERENCES [Users] ([Id]);

ALTER TABLE [InventoryReceiptInfo] ADD CONSTRAINT [FK_InventoryReceiptInfo_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [PurchaseRequest] ADD CONSTRAINT [FK_PurchaseRequest_Users_RejectedBy] FOREIGN KEY ([RejectedBy]) REFERENCES [Users] ([Id]);

ALTER TABLE [PurchaseRequest] ADD CONSTRAINT [FK_PurchaseRequest_Users_SentBy] FOREIGN KEY ([SentBy]) REFERENCES [Users] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260610143232_MajorSchemaOverhaulInventoryQuotationsBannerAndNews', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [ContactReply] DROP CONSTRAINT [FK_ContactReply_Users_RepliedById];

DECLARE @var17 nvarchar(max);
SELECT @var17 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContactReply]') AND [c].[name] = N'RepliedById');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [ContactReply] DROP CONSTRAINT ' + @var17 + ';');
ALTER TABLE [ContactReply] ALTER COLUMN [RepliedById] uniqueidentifier NULL;

ALTER TABLE [ContactReply] ADD [IsInternal] bit NOT NULL DEFAULT CAST(0 AS bit);

CREATE TABLE [CarrierPartners] (
    [Id] int NOT NULL IDENTITY,
    [CarrierCode] nvarchar(max) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [Environment] nvarchar(max) NOT NULL,
    [ApiBaseUrl] nvarchar(max) NOT NULL,
    [ApiToken] nvarchar(max) NOT NULL,
    [WebhookSecret] nvarchar(max) NOT NULL,
    [WebhookEndpointUrl] nvarchar(max) NOT NULL,
    [AutoSyncPricing] bit NOT NULL,
    [MaxParcelWeightKg] decimal(18,2) NOT NULL,
    [AllowLiquidCargo] bit NOT NULL,
    [AllowOversizeCargo] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_CarrierPartners] PRIMARY KEY ([Id])
);

CREATE TABLE [ContractTemplates] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [Code] nvarchar(max) NOT NULL,
    [Version] decimal(18,2) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [DynamicFields] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [Status] int NOT NULL,
    [ParentId] uniqueidentifier NULL,
    [IsUsed] bit NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ContractTemplates] PRIMARY KEY ([Id])
);

CREATE TABLE [CurrentUnreconciledCods] (
    [Id] int NOT NULL IDENTITY,
    [Value] decimal(18,2) NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_CurrentUnreconciledCods] PRIMARY KEY ([Id])
);

CREATE TABLE [CustomerFeedback] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [Rating] int NOT NULL,
    [FeedbackArea] nvarchar(50) NOT NULL,
    [CustomerName] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [Content] nvarchar(MAX) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_CustomerFeedback] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerFeedback_Contact_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contact] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Expenses] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(255) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [ExpenseDate] datetime2 NOT NULL,
    [Category] int NOT NULL,
    [Note] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id])
);

CREATE TABLE [FinanceContracts] (
    [Id] uniqueidentifier NOT NULL,
    [ContractNumber] nvarchar(max) NOT NULL,
    [CustomerId] uniqueidentifier NULL,
    [BankName] nvarchar(max) NOT NULL,
    [LoanAmount] decimal(18,2) NOT NULL,
    [TermMonths] int NOT NULL,
    [InterestRate] decimal(18,2) NOT NULL,
    [DisbursementStatus] nvarchar(max) NOT NULL,
    [CavetLocation] nvarchar(max) NOT NULL,
    [SignedDate] datetime2 NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_FinanceContracts] PRIMARY KEY ([Id])
);

CREATE TABLE [JobApplication] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [AppliedPosition] nvarchar(100) NOT NULL,
    [CvFileUrl] nvarchar(500) NOT NULL,
    [CoverLetter] nvarchar(MAX) NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_JobApplication] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JobApplication_Contact_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contact] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ParcelDeliveryOrders] (
    [Id] int NOT NULL IDENTITY,
    [TrackingNumber] nvarchar(max) NOT NULL,
    [CustomerName] nvarchar(max) NOT NULL,
    [Carrier] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpectedAt] datetime2 NULL,
    [DeliveredAt] datetime2 NULL,
    [CodAmount] decimal(18,2) NOT NULL,
    [ShippingCost] decimal(18,2) NOT NULL,
    [InspectedAt] datetime2 NULL,
    [ReturnReason] nvarchar(max) NULL,
    [BoxCondition] nvarchar(max) NULL,
    [ProductCondition] nvarchar(max) NULL,
    [ReturnProofImage] nvarchar(max) NULL,
    [ReturnInternalNote] nvarchar(max) NULL,
    [ReturnAction] nvarchar(max) NULL,
    [CustomerPhone] nvarchar(max) NOT NULL,
    [CustomerAddress] nvarchar(max) NOT NULL,
    [OriginalOrderCode] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_ParcelDeliveryOrders] PRIMARY KEY ([Id])
);

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
    [StartTime] datetimeoffset NULL,
    [ExpectedCompletionTime] datetimeoffset NULL,
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

CREATE TABLE [SalesContracts] (
    [Id] uniqueidentifier NOT NULL,
    [ContractNumber] nvarchar(100) NOT NULL,
    [OutputId] int NULL,
    [CustomerId] uniqueidentifier NULL,
    [ShowroomName] nvarchar(max) NULL,
    [ShowroomTaxCode] nvarchar(max) NULL,
    [ShowroomAddress] nvarchar(max) NULL,
    [ShowroomRepresentative] nvarchar(max) NULL,
    [CustomerFullName] nvarchar(max) NULL,
    [CustomerCCCD] nvarchar(max) NULL,
    [CustomerAddress] nvarchar(max) NULL,
    [CustomerPhone] nvarchar(max) NULL,
    [VehicleModel] nvarchar(max) NULL,
    [VehicleVersion] nvarchar(max) NULL,
    [VehicleColor] nvarchar(max) NULL,
    [FrameNumber] nvarchar(max) NULL,
    [EngineNumber] nvarchar(max) NULL,
    [ActualSalePrice] decimal(18,2) NOT NULL,
    [DepositAmount] decimal(18,2) NOT NULL,
    [RemainingAmount] decimal(18,2) NOT NULL,
    [FinalPaymentDeadline] datetimeoffset NULL,
    [WarrantyPeriod] nvarchar(max) NULL,
    [WarrantyScope] nvarchar(max) NULL,
    [SpecialTerms] nvarchar(max) NULL,
    [Status] nvarchar(max) NOT NULL,
    [SignedDate] datetimeoffset NULL,
    [ScannedFileUrl] nvarchar(max) NULL,
    [Note] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SalesContracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SalesContracts_Output_OutputId] FOREIGN KEY ([OutputId]) REFERENCES [Output] ([id]),
    CONSTRAINT [FK_SalesContracts_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id])
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

CREATE TABLE [SupplierContracts] (
    [Id] uniqueidentifier NOT NULL,
    [SupplierId] int NULL,
    [ContractNumber] nvarchar(100) NOT NULL,
    [ContractFilePath] nvarchar(500) NULL,
    [EffectiveDate] datetime2 NOT NULL,
    [ExpirationDate] datetime2 NULL,
    [ContractValue] decimal(18,2) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [Terms] nvarchar(500) NULL,
    [Note] nvarchar(1000) NULL,
    [CreditLimit] decimal(18,2) NULL,
    [PaymentWindowDays] int NULL,
    [BankAccountNumber] nvarchar(50) NULL,
    [BankName] nvarchar(200) NULL,
    [MinimumVolumePerMonth] int NULL,
    [DiscountRate] decimal(5,2) NULL,
    [ParentContractId] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupplierContracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupplierContracts_SupplierContracts_ParentContractId] FOREIGN KEY ([ParentContractId]) REFERENCES [SupplierContracts] ([Id]),
    CONSTRAINT [FK_SupplierContracts_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id])
);

CREATE TABLE [SupplierDebtSettlements] (
    [Id] uniqueidentifier NOT NULL,
    [SupplierId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaymentDate] datetimeoffset NOT NULL,
    [EvidenceUrl] nvarchar(500) NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupplierDebtSettlements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupplierDebtSettlements_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [SupplierFinances] (
    [SupplierId] int NOT NULL,
    [CurrentDebt] decimal(18,2) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupplierFinances] PRIMARY KEY ([SupplierId]),
    CONSTRAINT [FK_SupplierFinances_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [SupportRequest] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [Subject] nvarchar(200) NOT NULL,
    [Category] nvarchar(50) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [OrderCode] nvarchar(50) NULL,
    [Content] nvarchar(MAX) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [AssignedUserId] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupportRequest] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupportRequest_Contact_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contact] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ContractTemplateAuditLog] (
    [Id] uniqueidentifier NOT NULL,
    [ContractTemplateId] uniqueidentifier NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [Details] nvarchar(500) NULL,
    [ChangedBy] nvarchar(100) NULL,
    [IpAddress] nvarchar(50) NULL,
    [OldValue] nvarchar(2000) NULL,
    [NewValue] nvarchar(2000) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ContractTemplateAuditLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractTemplateAuditLog_ContractTemplates_ContractTemplateId] FOREIGN KEY ([ContractTemplateId]) REFERENCES [ContractTemplates] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ParcelDeliveryOrderItems] (
    [Id] int NOT NULL IDENTITY,
    [ParcelDeliveryOrderId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductName] nvarchar(max) NOT NULL,
    [Sku] nvarchar(max) NOT NULL,
    [ThumbnailUrl] nvarchar(max) NULL,
    [ShelfLocation] nvarchar(max) NOT NULL,
    [Quantity] int NOT NULL,
    [IsPicked] bit NOT NULL,
    [IsRestricted] bit NOT NULL,
    [IsOutOfStock] bit NOT NULL,
    CONSTRAINT [PK_ParcelDeliveryOrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ParcelDeliveryOrderItems_ParcelDeliveryOrders_ParcelDeliveryOrderId] FOREIGN KEY ([ParcelDeliveryOrderId]) REFERENCES [ParcelDeliveryOrders] ([Id]) ON DELETE CASCADE
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

CREATE TABLE [SupplierContractAuditLog] (
    [Id] uniqueidentifier NOT NULL,
    [SupplierContractId] uniqueidentifier NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [Details] nvarchar(500) NULL,
    [ChangedBy] nvarchar(100) NULL,
    [IpAddress] nvarchar(50) NULL,
    [OldValue] nvarchar(200) NULL,
    [NewValue] nvarchar(200) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupplierContractAuditLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupplierContractAuditLog_SupplierContracts_SupplierContractId] FOREIGN KEY ([SupplierContractId]) REFERENCES [SupplierContracts] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [SupplierContractItem] (
    [Id] uniqueidentifier NOT NULL,
    [SupplierContractId] uniqueidentifier NOT NULL,
    [ProductVariantId] int NOT NULL,
    [WholesalePrice] decimal(18,2) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupplierContractItem] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupplierContractItem_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SupplierContractItem_SupplierContracts_SupplierContractId] FOREIGN KEY ([SupplierContractId]) REFERENCES [SupplierContracts] ([Id]) ON DELETE CASCADE
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

CREATE TABLE [ServiceEvaluation] (
    [Id] int NOT NULL IDENTITY,
    [ServiceBookingId] int NOT NULL,
    [ContactId] int NOT NULL,
    [Criteria] nvarchar(30) NOT NULL,
    [Rating] int NOT NULL,
    [Review] nvarchar(MAX) NOT NULL,
    [ProcessingStatus] nvarchar(30) NOT NULL,
    [InternalNotes] nvarchar(MAX) NULL,
    [DirectReplyText] nvarchar(MAX) NULL,
    [AdminRepliedById] int NULL,
    [ProcessedAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ServiceEvaluation] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServiceEvaluation_Contact_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contact] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ServiceEvaluation_ServiceBooking_ServiceBookingId] FOREIGN KEY ([ServiceBookingId]) REFERENCES [ServiceBooking] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_ContractTemplateAuditLog_ContractTemplateId] ON [ContractTemplateAuditLog] ([ContractTemplateId]);

CREATE INDEX [IX_CustomerFeedback_ContactId] ON [CustomerFeedback] ([ContactId]);

CREATE INDEX [IX_JobApplication_ContactId] ON [JobApplication] ([ContactId]);

CREATE INDEX [IX_ParcelDeliveryOrderItems_ParcelDeliveryOrderId] ON [ParcelDeliveryOrderItems] ([ParcelDeliveryOrderId]);

CREATE INDEX [IX_PlateDossier_OutputId] ON [PlateDossier] ([OutputId]);

CREATE INDEX [IX_RepairOrder_TechnicianId] ON [RepairOrder] ([TechnicianId]);

CREATE INDEX [IX_RepairOrder_VehicleId] ON [RepairOrder] ([VehicleId]);

CREATE INDEX [IX_RepairOrderDetail_ProductVariantId] ON [RepairOrderDetail] ([ProductVariantId]);

CREATE INDEX [IX_RepairOrderDetail_RepairOrderId] ON [RepairOrderDetail] ([RepairOrderId]);

CREATE INDEX [IX_RepairOrderDetail_ServiceId] ON [RepairOrderDetail] ([ServiceId]);

CREATE INDEX [IX_SalesContracts_CustomerId] ON [SalesContracts] ([CustomerId]);

CREATE INDEX [IX_SalesContracts_OutputId] ON [SalesContracts] ([OutputId]);

CREATE INDEX [IX_ServiceBooking_CustomerId] ON [ServiceBooking] ([CustomerId]);

CREATE INDEX [IX_ServiceBooking_ServiceId] ON [ServiceBooking] ([ServiceId]);

CREATE INDEX [IX_ServiceBooking_TechnicianId] ON [ServiceBooking] ([TechnicianId]);

CREATE INDEX [IX_ServiceBooking_VehicleId] ON [ServiceBooking] ([VehicleId]);

CREATE INDEX [IX_ServiceEvaluation_ContactId] ON [ServiceEvaluation] ([ContactId]);

CREATE INDEX [IX_ServiceEvaluation_ServiceBookingId] ON [ServiceEvaluation] ([ServiceBookingId]);

CREATE INDEX [IX_Services_CategoryId] ON [Services] ([CategoryId]);

CREATE INDEX [IX_SupplierContractAuditLog_SupplierContractId] ON [SupplierContractAuditLog] ([SupplierContractId]);

CREATE INDEX [IX_SupplierContractItem_ProductVariantId] ON [SupplierContractItem] ([ProductVariantId]);

CREATE INDEX [IX_SupplierContractItem_SupplierContractId] ON [SupplierContractItem] ([SupplierContractId]);

CREATE INDEX [IX_SupplierContracts_ParentContractId] ON [SupplierContracts] ([ParentContractId]);

CREATE INDEX [IX_SupplierContracts_SupplierId] ON [SupplierContracts] ([SupplierId]);

CREATE INDEX [IX_SupplierDebtSettlements_SupplierId] ON [SupplierDebtSettlements] ([SupplierId]);

CREATE INDEX [IX_SupportRequest_ContactId] ON [SupportRequest] ([ContactId]);

ALTER TABLE [ContactReply] ADD CONSTRAINT [FK_ContactReply_Users_RepliedById] FOREIGN KEY ([RepliedById]) REFERENCES [Users] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260613024229_AddBusinessContractsAndServiceManagementModules', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [InventoryReceiptInfo] DROP CONSTRAINT [FK_InventoryReceiptInfo_Supplier_SupplierId];

ALTER TABLE [ServiceBooking] DROP CONSTRAINT [FK_ServiceBooking_EmployeeProfile_TechnicianId];

ALTER TABLE [ServiceBooking] DROP CONSTRAINT [FK_ServiceBooking_Services_ServiceId];

ALTER TABLE [ServiceBooking] DROP CONSTRAINT [FK_ServiceBooking_Users_CustomerId];

ALTER TABLE [ServiceBooking] DROP CONSTRAINT [FK_ServiceBooking_Vehicle_VehicleId];

ALTER TABLE [ServiceEvaluation] DROP CONSTRAINT [FK_ServiceEvaluation_ServiceBooking_ServiceBookingId];

ALTER TABLE [Vehicle] DROP CONSTRAINT [FK_Vehicle_Product_ProductId];

DROP TABLE [SupplierDebtSettlements];

DROP INDEX [IX_InventoryReceiptInfo_SupplierId] ON [InventoryReceiptInfo];

ALTER TABLE [ServiceBooking] DROP CONSTRAINT [PK_ServiceBooking];

DROP INDEX [IX_ServiceBooking_ServiceId] ON [ServiceBooking];

DROP INDEX [IX_ServiceBooking_TechnicianId] ON [ServiceBooking];

DECLARE @var18 nvarchar(max);
SELECT @var18 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InventoryReceiptInfo]') AND [c].[name] = N'SupplierId');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [InventoryReceiptInfo] DROP CONSTRAINT ' + @var18 + ';');
ALTER TABLE [InventoryReceiptInfo] DROP COLUMN [SupplierId];

DECLARE @var19 nvarchar(max);
SELECT @var19 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InventoryReceiptInfo]') AND [c].[name] = N'UnitPrice');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [InventoryReceiptInfo] DROP CONSTRAINT ' + @var19 + ';');
ALTER TABLE [InventoryReceiptInfo] DROP COLUMN [UnitPrice];

DECLARE @var20 nvarchar(max);
SELECT @var20 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'CancelledDate');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var20 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [CancelledDate];

DECLARE @var21 nvarchar(max);
SELECT @var21 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'CancelledReason');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var21 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [CancelledReason];

DECLARE @var22 nvarchar(max);
SELECT @var22 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'CompletedDate');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var22 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [CompletedDate];

DECLARE @var23 nvarchar(max);
SELECT @var23 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'CustomerNotes');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var23 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [CustomerNotes];

DECLARE @var24 nvarchar(max);
SELECT @var24 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'DepositAmount');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var24 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [DepositAmount];

DECLARE @var25 nvarchar(max);
SELECT @var25 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'EstimatedDurationMinutes');
IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var25 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [EstimatedDurationMinutes];

DECLARE @var26 nvarchar(max);
SELECT @var26 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'PaymentStatus');
IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var26 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [PaymentStatus];

DECLARE @var27 nvarchar(max);
SELECT @var27 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'Rating');
IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var27 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [Rating];

DECLARE @var28 nvarchar(max);
SELECT @var28 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'Review');
IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var28 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [Review];

DECLARE @var29 nvarchar(max);
SELECT @var29 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'ScheduledDate');
IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var29 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [ScheduledDate];

DECLARE @var30 nvarchar(max);
SELECT @var30 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'ServiceId');
IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var30 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [ServiceId];

DECLARE @var31 nvarchar(max);
SELECT @var31 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'TechnicianId');
IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var31 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [TechnicianId];

DECLARE @var32 nvarchar(max);
SELECT @var32 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'TechnicianNotes');
IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var32 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [TechnicianNotes];

DECLARE @var33 nvarchar(max);
SELECT @var33 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'TotalAmount');
IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var33 + ';');
ALTER TABLE [ServiceBooking] DROP COLUMN [TotalAmount];

EXEC sp_rename N'[ServiceBooking]', N'ServiceBookings', 'OBJECT';

EXEC sp_rename N'[ServiceBookings].[CustomerId]', N'AssignedSaleId', 'COLUMN';

EXEC sp_rename N'[ServiceBookings].[IX_ServiceBooking_VehicleId]', N'IX_ServiceBookings_VehicleId', 'INDEX';

EXEC sp_rename N'[ServiceBookings].[IX_ServiceBooking_CustomerId]', N'IX_ServiceBookings_AssignedSaleId', 'INDEX';

ALTER TABLE [Vehicle] ADD [CurrentOdo] float NOT NULL DEFAULT 0.0E0;

ALTER TABLE [Vehicle] ADD [ElectronicWarrantyQrCode] nvarchar(255) NOT NULL DEFAULT N'';

ALTER TABLE [Vehicle] ADD [LastMaintenanceDate] datetime2 NULL;

ALTER TABLE [Vehicle] ADD [NextMaintenanceDate] datetime2 NULL;

ALTER TABLE [Vehicle] ADD [NextMaintenanceOdo] float NULL;

ALTER TABLE [Vehicle] ADD [UserId] uniqueidentifier NULL;

ALTER TABLE [PurchaseRequestItem] ADD [ProductQuotationId] int NULL;

ALTER TABLE [PurchaseRequestItem] ADD [SupplierId] int NULL;

ALTER TABLE [PurchaseRequestItem] ADD [UnitPrice] decimal(18,2) NULL;

ALTER TABLE [ProductVariantColor] ADD [MaxPurchaseQuantity] int NULL;

ALTER TABLE [ProductVariant] ADD [MaxPurchaseQuantity] int NULL;

ALTER TABLE [Product] ADD [MaxPurchaseQuantity] int NULL;

ALTER TABLE [Lead] ADD [Notes] nvarchar(MAX) NOT NULL DEFAULT N'';

ALTER TABLE [Lead] ADD [Priority] nvarchar(20) NOT NULL DEFAULT N'';

ALTER TABLE [InventoryOnHand] ADD [BeginningQty] int NOT NULL DEFAULT 0;

ALTER TABLE [InventoryOnHand] ADD [Month] int NOT NULL DEFAULT 0;

ALTER TABLE [InventoryOnHand] ADD [Year] int NOT NULL DEFAULT 0;

DROP INDEX [IX_ServiceBookings_VehicleId] ON [ServiceBookings];
DECLARE @var34 nvarchar(max);
SELECT @var34 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBookings]') AND [c].[name] = N'VehicleId');
IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBookings] DROP CONSTRAINT ' + @var34 + ';');
UPDATE [ServiceBookings] SET [VehicleId] = 0 WHERE [VehicleId] IS NULL;
ALTER TABLE [ServiceBookings] ALTER COLUMN [VehicleId] int NOT NULL;
ALTER TABLE [ServiceBookings] ADD DEFAULT 0 FOR [VehicleId];
CREATE INDEX [IX_ServiceBookings_VehicleId] ON [ServiceBookings] ([VehicleId]);

DECLARE @var35 nvarchar(max);
SELECT @var35 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBookings]') AND [c].[name] = N'Status');
IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBookings] DROP CONSTRAINT ' + @var35 + ';');
ALTER TABLE [ServiceBookings] ALTER COLUMN [Status] nvarchar(max) NOT NULL;

DECLARE @var36 nvarchar(max);
SELECT @var36 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBookings]') AND [c].[name] = N'Notes');
IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBookings] DROP CONSTRAINT ' + @var36 + ';');
UPDATE [ServiceBookings] SET [Notes] = N'' WHERE [Notes] IS NULL;
ALTER TABLE [ServiceBookings] ALTER COLUMN [Notes] nvarchar(max) NOT NULL;
ALTER TABLE [ServiceBookings] ADD DEFAULT N'' FOR [Notes];

ALTER TABLE [ServiceBookings] ADD [AdminNote] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [ServiceBookings] ADD [AppointmentDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [ServiceBookings] ADD [AppointmentTime] time NOT NULL DEFAULT '00:00:00';

ALTER TABLE [ServiceBookings] ADD [CancellationReason] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [ServiceBookings] ADD [CancelledAt] datetime2 NULL;

ALTER TABLE [ServiceBookings] ADD [CustomerNote] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [ServiceBookings] ADD [ServiceType] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [ServiceBookings] ADD CONSTRAINT [PK_ServiceBookings] PRIMARY KEY ([Id]);

CREATE TABLE [BookingAppointment] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(150) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Email] nvarchar(150) NULL,
    [ProductVariantId] int NULL,
    [PreferredDate] datetime2 NULL,
    [PreferredTimeSlot] nvarchar(100) NULL,
    [AppointmentAt] datetimeoffset NULL,
    [Showroom] nvarchar(150) NULL,
    [Status] nvarchar(30) NOT NULL,
    [Notes] nvarchar(MAX) NULL,
    [ConfirmedAt] datetimeoffset NULL,
    [ConfirmedBy] uniqueidentifier NULL,
    [CancelReason] nvarchar(500) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_BookingAppointment] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BookingAppointment_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_BookingAppointment_Users_ConfirmedBy] FOREIGN KEY ([ConfirmedBy]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [CustomerContact] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(150) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Email] nvarchar(150) NULL,
    [Subject] nvarchar(200) NULL,
    [Message] nvarchar(MAX) NOT NULL,
    [Status] nvarchar(30) NOT NULL,
    [Source] nvarchar(30) NULL,
    [ProcessedAt] datetimeoffset NULL,
    [ProcessedBy] uniqueidentifier NULL,
    [RepliedAt] datetimeoffset NULL,
    [InternalNote] nvarchar(MAX) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_CustomerContact] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerContact_Users_ProcessedBy] FOREIGN KEY ([ProcessedBy]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [InventoryReceiptAuditLog] (
    [Id] int NOT NULL IDENTITY,
    [InventoryReceiptId] int NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [ChangedById] uniqueidentifier NULL,
    [ChangedAt] datetimeoffset NOT NULL,
    [OldStatusId] nvarchar(max) NULL,
    [NewStatusId] nvarchar(max) NULL,
    [OldNotes] nvarchar(max) NULL,
    [NewNotes] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_InventoryReceiptAuditLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryReceiptAuditLog_InventoryReceipt_InventoryReceiptId] FOREIGN KEY ([InventoryReceiptId]) REFERENCES [InventoryReceipt] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InventoryReceiptAuditLog_Users_ChangedById] FOREIGN KEY ([ChangedById]) REFERENCES [Users] ([Id])
);

CREATE TABLE [InventoryReceiptInfoAuditLog] (
    [Id] int NOT NULL IDENTITY,
    [InventoryReceiptInfoId] int NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [OldQuantity] int NULL,
    [NewQuantity] int NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_InventoryReceiptInfoAuditLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryReceiptInfoAuditLog_InventoryReceiptInfo_InventoryReceiptInfoId] FOREIGN KEY ([InventoryReceiptInfoId]) REFERENCES [InventoryReceiptInfo] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [InventoryTransaction] (
    [Id] int NOT NULL IDENTITY,
    [ProductVariantId] int NOT NULL,
    [TransactionType] nvarchar(30) NOT NULL,
    [Quantity] int NOT NULL,
    [StockBefore] int NOT NULL,
    [StockAfter] int NOT NULL,
    [ReferenceType] nvarchar(30) NULL,
    [ReferenceId] int NULL,
    [Note] nvarchar(500) NULL,
    [PerformedBy] uniqueidentifier NULL,
    [PerformedAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_InventoryTransaction] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryTransaction_ProductVariant_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariant] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InventoryTransaction_Users_PerformedBy] FOREIGN KEY ([PerformedBy]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [NewsArticle] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(255) NOT NULL,
    [Slug] nvarchar(255) NOT NULL,
    [Excerpt] nvarchar(500) NULL,
    [Content] nvarchar(MAX) NULL,
    [CoverImageUrl] nvarchar(500) NULL,
    [SeoTitle] nvarchar(255) NULL,
    [SeoDescription] nvarchar(500) NULL,
    [SeoKeywords] nvarchar(500) NULL,
    [Status] nvarchar(30) NOT NULL,
    [IsFeatured] bit NOT NULL,
    [ViewCount] int NOT NULL,
    [PublishedAt] datetimeoffset NULL,
    [PublishedBy] uniqueidentifier NULL,
    [AuthorId] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_NewsArticle] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_NewsArticle_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_NewsArticle_Users_PublishedBy] FOREIGN KEY ([PublishedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [OrderLogistics] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [CurrentStage] int NOT NULL,
    [BottleneckDescription] nvarchar(max) NOT NULL,
    [IsBottleneck] bit NOT NULL,
    [DriverName] nvarchar(max) NOT NULL,
    [DriverPhone] nvarchar(max) NOT NULL,
    [CurrentLat] float NOT NULL,
    [CurrentLng] float NOT NULL,
    [EstimatedArrivalTime] datetime2 NULL,
    [LastUpdated] datetime2 NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_OrderLogistics] PRIMARY KEY ([Id])
);

CREATE TABLE [OrderStatusHistory] (
    [Id] int NOT NULL IDENTITY,
    [OutputId] int NOT NULL,
    [FromStatus] nvarchar(50) NULL,
    [ToStatus] nvarchar(50) NOT NULL,
    [ChangedBy] uniqueidentifier NULL,
    [ChangedAt] datetimeoffset NULL,
    [Note] nvarchar(500) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_OrderStatusHistory] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderStatusHistory_Output_OutputId] FOREIGN KEY ([OutputId]) REFERENCES [Output] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderStatusHistory_Users_ChangedBy] FOREIGN KEY ([ChangedBy]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [PromotionBanner] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(150) NOT NULL,
    [Description] nvarchar(500) NULL,
    [ImageUrl] nvarchar(500) NULL,
    [LinkUrl] nvarchar(500) NULL,
    [StartDate] datetimeoffset NOT NULL,
    [EndDate] datetimeoffset NOT NULL,
    [SortOrder] int NOT NULL,
    [IsEnabled] bit NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_PromotionBanner] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PromotionBanner_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_PromotionBanner_Users_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PurchaseRequestAuditLog] (
    [Id] int NOT NULL IDENTITY,
    [PurchaseRequestId] int NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [ChangedById] uniqueidentifier NULL,
    [ChangedAt] datetimeoffset NOT NULL,
    [OldStatusId] nvarchar(max) NULL,
    [NewStatusId] nvarchar(max) NULL,
    [OldNotes] nvarchar(max) NULL,
    [NewNotes] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_PurchaseRequestAuditLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchaseRequestAuditLog_PurchaseRequest_PurchaseRequestId] FOREIGN KEY ([PurchaseRequestId]) REFERENCES [PurchaseRequest] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PurchaseRequestAuditLog_Users_ChangedById] FOREIGN KEY ([ChangedById]) REFERENCES [Users] ([Id])
);

CREATE TABLE [PurchaseRequestItemAuditLog] (
    [Id] int NOT NULL IDENTITY,
    [PurchaseRequestItemId] int NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [OldQuantity] int NULL,
    [NewQuantity] int NULL,
    [OldProductVariantId] int NULL,
    [NewProductVariantId] int NULL,
    [OldProductVariantColorId] int NULL,
    [NewProductVariantColorId] int NULL,
    [OldSupplierName] nvarchar(max) NULL,
    [NewSupplierName] nvarchar(max) NULL,
    [OldUnitPrice] decimal(18,2) NULL,
    [NewUnitPrice] decimal(18,2) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_PurchaseRequestItemAuditLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchaseRequestItemAuditLog_PurchaseRequestItem_PurchaseRequestItemId] FOREIGN KEY ([PurchaseRequestItemId]) REFERENCES [PurchaseRequestItem] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [SupplierDebtLog] (
    [Id] int NOT NULL IDENTITY,
    [SupplierId] int NOT NULL,
    [AmountPaid] decimal(18,2) NOT NULL,
    [RemainingDebt] decimal(18,2) NOT NULL,
    [PaymentDate] datetimeoffset NOT NULL,
    [CreatedById] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupplierDebtLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupplierDebtLog_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SupplierDebtLog_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id])
);

CREATE TABLE [SupportTicket] (
    [Id] int NOT NULL IDENTITY,
    [CustomerId] uniqueidentifier NULL,
    [Subject] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Priority] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [ResolvedAt] datetime2 NULL,
    [SLADeadline] datetime2 NOT NULL,
    [AssignedAdminId] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupportTicket] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupportTicket_Users_AssignedAdminId] FOREIGN KEY ([AssignedAdminId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_SupportTicket_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [VehicleAuditLog] (
    [Id] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [ChangedById] uniqueidentifier NULL,
    [ChangedAt] datetimeoffset NOT NULL,
    [OldVinNumber] nvarchar(max) NULL,
    [NewVinNumber] nvarchar(max) NULL,
    [OldEngineNumber] nvarchar(max) NULL,
    [NewEngineNumber] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_VehicleAuditLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VehicleAuditLog_Users_ChangedById] FOREIGN KEY ([ChangedById]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_VehicleAuditLog_Vehicle_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicle] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CustomerContactReply] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [ReplyContent] nvarchar(MAX) NOT NULL,
    [IsInternal] bit NOT NULL,
    [RepliedBy] uniqueidentifier NULL,
    [SentAt] datetimeoffset NULL,
    [SupportTicketId] int NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_CustomerContactReply] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerContactReply_CustomerContact_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [CustomerContact] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CustomerContactReply_SupportTicket_SupportTicketId] FOREIGN KEY ([SupportTicketId]) REFERENCES [SupportTicket] ([Id]),
    CONSTRAINT [FK_CustomerContactReply_Users_RepliedBy] FOREIGN KEY ([RepliedBy]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);

CREATE INDEX [IX_Vehicle_UserId] ON [Vehicle] ([UserId]);

CREATE INDEX [IX_PurchaseRequestItem_ProductQuotationId] ON [PurchaseRequestItem] ([ProductQuotationId]);

CREATE INDEX [IX_PurchaseRequestItem_SupplierId] ON [PurchaseRequestItem] ([SupplierId]);

CREATE UNIQUE INDEX [IX_InventoryOnHand_ProductVariantId_ProductVariantColorId_Month_Year] ON [InventoryOnHand] ([ProductVariantId], [ProductVariantColorId], [Month], [Year]) WHERE [ProductVariantColorId] IS NOT NULL;

CREATE INDEX [IX_BookingAppointment_ConfirmedBy] ON [BookingAppointment] ([ConfirmedBy]);

CREATE INDEX [IX_BookingAppointment_ProductVariantId] ON [BookingAppointment] ([ProductVariantId]);

CREATE INDEX [IX_BookingAppointment_Status_AppointmentAt] ON [BookingAppointment] ([Status], [AppointmentAt]);

CREATE INDEX [IX_CustomerContact_ProcessedBy] ON [CustomerContact] ([ProcessedBy]);

CREATE INDEX [IX_CustomerContact_Status_CreatedAt] ON [CustomerContact] ([Status], [CreatedAt]);

CREATE INDEX [IX_CustomerContactReply_ContactId_SentAt] ON [CustomerContactReply] ([ContactId], [SentAt]);

CREATE INDEX [IX_CustomerContactReply_RepliedBy] ON [CustomerContactReply] ([RepliedBy]);

CREATE INDEX [IX_CustomerContactReply_SupportTicketId] ON [CustomerContactReply] ([SupportTicketId]);

CREATE INDEX [IX_InventoryReceiptAuditLog_ChangedById] ON [InventoryReceiptAuditLog] ([ChangedById]);

CREATE INDEX [IX_InventoryReceiptAuditLog_InventoryReceiptId] ON [InventoryReceiptAuditLog] ([InventoryReceiptId]);

CREATE INDEX [IX_InventoryReceiptInfoAuditLog_InventoryReceiptInfoId] ON [InventoryReceiptInfoAuditLog] ([InventoryReceiptInfoId]);

CREATE INDEX [IX_InventoryTransaction_PerformedBy] ON [InventoryTransaction] ([PerformedBy]);

CREATE INDEX [IX_InventoryTransaction_ProductVariantId_PerformedAt] ON [InventoryTransaction] ([ProductVariantId], [PerformedAt]);

CREATE INDEX [IX_NewsArticle_AuthorId] ON [NewsArticle] ([AuthorId]);

CREATE INDEX [IX_NewsArticle_PublishedBy] ON [NewsArticle] ([PublishedBy]);

CREATE UNIQUE INDEX [IX_NewsArticle_Slug] ON [NewsArticle] ([Slug]);

CREATE INDEX [IX_NewsArticle_Status_PublishedAt] ON [NewsArticle] ([Status], [PublishedAt]);

CREATE INDEX [IX_OrderStatusHistory_ChangedBy] ON [OrderStatusHistory] ([ChangedBy]);

CREATE INDEX [IX_OrderStatusHistory_OutputId_ChangedAt] ON [OrderStatusHistory] ([OutputId], [ChangedAt]);

CREATE INDEX [IX_PromotionBanner_CreatedBy] ON [PromotionBanner] ([CreatedBy]);

CREATE INDEX [IX_PromotionBanner_IsEnabled_StartDate_EndDate] ON [PromotionBanner] ([IsEnabled], [StartDate], [EndDate]);

CREATE INDEX [IX_PromotionBanner_UpdatedBy] ON [PromotionBanner] ([UpdatedBy]);

CREATE INDEX [IX_PurchaseRequestAuditLog_ChangedById] ON [PurchaseRequestAuditLog] ([ChangedById]);

CREATE INDEX [IX_PurchaseRequestAuditLog_PurchaseRequestId] ON [PurchaseRequestAuditLog] ([PurchaseRequestId]);

CREATE INDEX [IX_PurchaseRequestItemAuditLog_PurchaseRequestItemId] ON [PurchaseRequestItemAuditLog] ([PurchaseRequestItemId]);

CREATE INDEX [IX_SupplierDebtLog_CreatedById] ON [SupplierDebtLog] ([CreatedById]);

CREATE INDEX [IX_SupplierDebtLog_SupplierId] ON [SupplierDebtLog] ([SupplierId]);

CREATE INDEX [IX_SupportTicket_AssignedAdminId] ON [SupportTicket] ([AssignedAdminId]);

CREATE INDEX [IX_SupportTicket_CustomerId] ON [SupportTicket] ([CustomerId]);

CREATE INDEX [IX_VehicleAuditLog_ChangedById] ON [VehicleAuditLog] ([ChangedById]);

CREATE INDEX [IX_VehicleAuditLog_VehicleId] ON [VehicleAuditLog] ([VehicleId]);

ALTER TABLE [PurchaseRequestItem] ADD CONSTRAINT [FK_PurchaseRequestItem_ProductQuotations_ProductQuotationId] FOREIGN KEY ([ProductQuotationId]) REFERENCES [ProductQuotations] ([Id]);

ALTER TABLE [PurchaseRequestItem] ADD CONSTRAINT [FK_PurchaseRequestItem_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id]);

ALTER TABLE [ServiceBookings] ADD CONSTRAINT [FK_ServiceBookings_Users_AssignedSaleId] FOREIGN KEY ([AssignedSaleId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;

ALTER TABLE [ServiceBookings] ADD CONSTRAINT [FK_ServiceBookings_Vehicle_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicle] ([Id]) ON DELETE CASCADE;

ALTER TABLE [ServiceEvaluation] ADD CONSTRAINT [FK_ServiceEvaluation_ServiceBookings_ServiceBookingId] FOREIGN KEY ([ServiceBookingId]) REFERENCES [ServiceBookings] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Vehicle] ADD CONSTRAINT [FK_Vehicle_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Product] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Vehicle] ADD CONSTRAINT [FK_Vehicle_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260624123942_UpgradeInventoryServiceBookingAndAddCrmCmsModules', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [ServiceBookings] DROP CONSTRAINT [FK_ServiceBookings_Users_AssignedSaleId];

ALTER TABLE [ServiceBookings] DROP CONSTRAINT [FK_ServiceBookings_Vehicle_VehicleId];

ALTER TABLE [ServiceEvaluation] DROP CONSTRAINT [FK_ServiceEvaluation_ServiceBookings_ServiceBookingId];

ALTER TABLE [ServiceBookings] DROP CONSTRAINT [PK_ServiceBookings];

DECLARE @var37 nvarchar(max);
SELECT @var37 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBookings]') AND [c].[name] = N'AdminNote');
IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBookings] DROP CONSTRAINT ' + @var37 + ';');
ALTER TABLE [ServiceBookings] DROP COLUMN [AdminNote];

DECLARE @var38 nvarchar(max);
SELECT @var38 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBookings]') AND [c].[name] = N'AppointmentDate');
IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBookings] DROP CONSTRAINT ' + @var38 + ';');
ALTER TABLE [ServiceBookings] DROP COLUMN [AppointmentDate];

DECLARE @var39 nvarchar(max);
SELECT @var39 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBookings]') AND [c].[name] = N'AppointmentTime');
IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBookings] DROP CONSTRAINT ' + @var39 + ';');
ALTER TABLE [ServiceBookings] DROP COLUMN [AppointmentTime];

DECLARE @var40 nvarchar(max);
SELECT @var40 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBookings]') AND [c].[name] = N'CancellationReason');
IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBookings] DROP CONSTRAINT ' + @var40 + ';');
ALTER TABLE [ServiceBookings] DROP COLUMN [CancellationReason];

DECLARE @var41 nvarchar(max);
SELECT @var41 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBookings]') AND [c].[name] = N'CancelledAt');
IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBookings] DROP CONSTRAINT ' + @var41 + ';');
ALTER TABLE [ServiceBookings] DROP COLUMN [CancelledAt];

DECLARE @var42 nvarchar(max);
SELECT @var42 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBookings]') AND [c].[name] = N'CustomerNote');
IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBookings] DROP CONSTRAINT ' + @var42 + ';');
ALTER TABLE [ServiceBookings] DROP COLUMN [CustomerNote];

DECLARE @var43 nvarchar(max);
SELECT @var43 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBookings]') AND [c].[name] = N'ServiceType');
IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBookings] DROP CONSTRAINT ' + @var43 + ';');
ALTER TABLE [ServiceBookings] DROP COLUMN [ServiceType];

EXEC sp_rename N'[ServiceBookings]', N'ServiceBooking', 'OBJECT';

EXEC sp_rename N'[ServiceBooking].[AssignedSaleId]', N'CustomerId', 'COLUMN';

EXEC sp_rename N'[ServiceBooking].[IX_ServiceBookings_VehicleId]', N'IX_ServiceBooking_VehicleId', 'INDEX';

EXEC sp_rename N'[ServiceBooking].[IX_ServiceBookings_AssignedSaleId]', N'IX_ServiceBooking_CustomerId', 'INDEX';

DECLARE @var44 nvarchar(max);
SELECT @var44 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'VehicleId');
IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var44 + ';');
ALTER TABLE [ServiceBooking] ALTER COLUMN [VehicleId] int NULL;

DECLARE @var45 nvarchar(max);
SELECT @var45 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'Status');
IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var45 + ';');
ALTER TABLE [ServiceBooking] ALTER COLUMN [Status] nvarchar(20) NOT NULL;

DECLARE @var46 nvarchar(max);
SELECT @var46 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceBooking]') AND [c].[name] = N'Notes');
IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [ServiceBooking] DROP CONSTRAINT ' + @var46 + ';');
ALTER TABLE [ServiceBooking] ALTER COLUMN [Notes] nvarchar(MAX) NULL;

ALTER TABLE [ServiceBooking] ADD [CancelledDate] datetimeoffset NULL;

ALTER TABLE [ServiceBooking] ADD [CancelledReason] nvarchar(500) NULL;

ALTER TABLE [ServiceBooking] ADD [CompletedDate] datetimeoffset NULL;

ALTER TABLE [ServiceBooking] ADD [CustomerNotes] nvarchar(MAX) NULL;

ALTER TABLE [ServiceBooking] ADD [DepositAmount] decimal(18,2) NULL;

ALTER TABLE [ServiceBooking] ADD [EstimatedDurationMinutes] int NULL;

ALTER TABLE [ServiceBooking] ADD [PaymentStatus] nvarchar(20) NOT NULL DEFAULT N'';

ALTER TABLE [ServiceBooking] ADD [Rating] int NULL;

ALTER TABLE [ServiceBooking] ADD [Review] nvarchar(MAX) NULL;

ALTER TABLE [ServiceBooking] ADD [ScheduledDate] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';

ALTER TABLE [ServiceBooking] ADD [ServiceId] int NOT NULL DEFAULT 0;

ALTER TABLE [ServiceBooking] ADD [TechnicianId] int NULL;

ALTER TABLE [ServiceBooking] ADD [TechnicianNotes] nvarchar(MAX) NULL;

ALTER TABLE [ServiceBooking] ADD [TotalAmount] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [ServiceBooking] ADD CONSTRAINT [PK_ServiceBooking] PRIMARY KEY ([Id]);

CREATE TABLE [SupplierDebtSettlements] (
    [Id] uniqueidentifier NOT NULL,
    [SupplierId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaymentDate] datetimeoffset NOT NULL,
    [EvidenceUrl] nvarchar(500) NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupplierDebtSettlements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupplierDebtSettlements_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_ServiceBooking_ServiceId] ON [ServiceBooking] ([ServiceId]);

CREATE INDEX [IX_ServiceBooking_TechnicianId] ON [ServiceBooking] ([TechnicianId]);

CREATE INDEX [IX_SupplierDebtSettlements_SupplierId] ON [SupplierDebtSettlements] ([SupplierId]);

ALTER TABLE [ServiceBooking] ADD CONSTRAINT [FK_ServiceBooking_EmployeeProfile_TechnicianId] FOREIGN KEY ([TechnicianId]) REFERENCES [EmployeeProfile] ([Id]) ON DELETE SET NULL;

ALTER TABLE [ServiceBooking] ADD CONSTRAINT [FK_ServiceBooking_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE CASCADE;

ALTER TABLE [ServiceBooking] ADD CONSTRAINT [FK_ServiceBooking_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]);

ALTER TABLE [ServiceBooking] ADD CONSTRAINT [FK_ServiceBooking_Vehicle_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicle] ([Id]) ON DELETE CASCADE;

ALTER TABLE [ServiceEvaluation] ADD CONSTRAINT [FK_ServiceEvaluation_ServiceBooking_ServiceBookingId] FOREIGN KEY ([ServiceBookingId]) REFERENCES [ServiceBooking] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260625113447_RefactorServiceBookingAndAddSupplierDebt', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [PlateDossier] DROP CONSTRAINT [FK_PlateDossier_Output_OutputId];

DROP TABLE [ContractTemplateAuditLog];

DROP TABLE [ServiceEvaluation];

DROP TABLE [ContractTemplates];

DECLARE @var47 nvarchar(max);
SELECT @var47 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PlateDossier]') AND [c].[name] = N'Status');
IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [PlateDossier] DROP CONSTRAINT ' + @var47 + ';');
ALTER TABLE [PlateDossier] ALTER COLUMN [Status] nvarchar(50) NOT NULL;

DECLARE @var48 nvarchar(max);
SELECT @var48 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PlateDossier]') AND [c].[name] = N'OutputId');
IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [PlateDossier] DROP CONSTRAINT ' + @var48 + ';');
ALTER TABLE [PlateDossier] ALTER COLUMN [OutputId] int NULL;

ALTER TABLE [PlateDossier] ADD [CompletedDate] datetimeoffset NULL;

ALTER TABLE [PlateDossier] ADD [CustomerName] nvarchar(100) NOT NULL DEFAULT N'';

ALTER TABLE [PlateDossier] ADD [CustomerPhone] nvarchar(20) NOT NULL DEFAULT N'';

ALTER TABLE [PlateDossier] ADD [DossierNumber] nvarchar(50) NOT NULL DEFAULT N'';

ALTER TABLE [PlateDossier] ADD [VinNumber] nvarchar(50) NOT NULL DEFAULT N'';

ALTER TABLE [ParcelDeliveryOrders] ADD [RefundAmount] decimal(18,2) NULL;

ALTER TABLE [ParcelDeliveryOrders] ADD [RejectionReason] nvarchar(max) NULL;

ALTER TABLE [ParcelDeliveryOrders] ADD [ReturnShippingCost] decimal(18,2) NULL;

ALTER TABLE [Output] ADD [LeadId] int NULL;

ALTER TABLE [MaintenanceHistory] ADD [LaborCost] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [MaintenanceHistory] ADD [MaintenanceNumber] nvarchar(50) NOT NULL DEFAULT N'';

ALTER TABLE [MaintenanceHistory] ADD [NextMaintenanceDate] datetimeoffset NULL;

ALTER TABLE [MaintenanceHistory] ADD [NextMaintenanceOdo] int NULL;

ALTER TABLE [MaintenanceHistory] ADD [PartsCost] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [MaintenanceHistory] ADD [PartsJson] nvarchar(MAX) NULL;

ALTER TABLE [MaintenanceHistory] ADD [TechnicianId] int NULL;

ALTER TABLE [MaintenanceHistory] ADD [TotalCost] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Lead] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);

DECLARE @var49 nvarchar(max);
SELECT @var49 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CustomerFeedback]') AND [c].[name] = N'FeedbackArea');
IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [CustomerFeedback] DROP CONSTRAINT ' + @var49 + ';');
ALTER TABLE [CustomerFeedback] ALTER COLUMN [FeedbackArea] nvarchar(250) NOT NULL;

ALTER TABLE [CarrierPartners] ADD [PricingRulesJson] nvarchar(max) NULL;

ALTER TABLE [CarrierPartners] ADD [SlaJson] nvarchar(max) NULL;

CREATE TABLE [ConversionTool] (
    [Id] int NOT NULL IDENTITY,
    [Type] nvarchar(20) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Content] nvarchar(MAX) NULL,
    [DelaySeconds] int NULL,
    [Pages] nvarchar(MAX) NULL,
    [IsActive] bit NOT NULL,
    [Views] int NOT NULL,
    [Clicks] int NOT NULL,
    [ImageUrl] nvarchar(500) NULL,
    [Url] nvarchar(500) NULL,
    [Status] nvarchar(20) NULL,
    [Leads] int NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ConversionTool] PRIMARY KEY ([Id])
);

CREATE TABLE [Invoice] (
    [Id] int NOT NULL IDENTITY,
    [InvoiceNumber] nvarchar(max) NOT NULL,
    [IssueDate] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CustomerName] nvarchar(max) NOT NULL,
    [CustomerIdCard] nvarchar(max) NOT NULL,
    [CustomerPhone] nvarchar(max) NOT NULL,
    [CustomerAddress] nvarchar(max) NOT NULL,
    [VehicleModel] nvarchar(max) NOT NULL,
    [VehicleColor] nvarchar(max) NOT NULL,
    [ChassisNo] nvarchar(max) NOT NULL,
    [EngineNo] nvarchar(max) NOT NULL,
    [VehiclePrice] decimal(18,2) NOT NULL,
    [RegistrationFee] decimal(18,2) NOT NULL,
    [InsuranceFee] decimal(18,2) NOT NULL,
    [PaymentMethod] nvarchar(max) NOT NULL,
    [BankName] nvarchar(max) NULL,
    [Status] nvarchar(max) NOT NULL,
    [ProcessedBy] nvarchar(max) NULL,
    [ProcessedAt] datetime2 NULL,
    [SalesPerson] nvarchar(max) NOT NULL,
    [DeliveryDate] datetime2 NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Invoice] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invoice_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PurchaseInvoice] (
    [Id] int NOT NULL IDENTITY,
    [InvoiceNumber] nvarchar(50) NOT NULL,
    [PurchaseRequestId] int NULL,
    [SupplierId] int NULL,
    [SupplierName] nvarchar(200) NULL,
    [SupplierPhone] nvarchar(20) NULL,
    [SupplierAddress] nvarchar(500) NULL,
    [SupplierTaxCode] nvarchar(50) NULL,
    [CustomerName] nvarchar(200) NULL,
    [CustomerPhone] nvarchar(20) NULL,
    [CustomerAddress] nvarchar(500) NULL,
    [CustomerIdCard] nvarchar(30) NULL,
    [InvoiceDate] datetimeoffset NOT NULL,
    [DueDate] datetimeoffset NULL,
    [Status] nvarchar(30) NOT NULL,
    [SubTotal] decimal(18,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [PaymentMethod] nvarchar(30) NULL,
    [PaymentStatus] nvarchar(30) NULL,
    [PaidAt] datetimeoffset NULL,
    [Notes] nvarchar(MAX) NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_PurchaseInvoice] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchaseInvoice_PurchaseRequest_PurchaseRequestId] FOREIGN KEY ([PurchaseRequestId]) REFERENCES [PurchaseRequest] ([Id]),
    CONSTRAINT [FK_PurchaseInvoice_Supplier_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Supplier] ([Id])
);

CREATE TABLE [ReturnRequest] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [OrderCode] nvarchar(max) NOT NULL,
    [OriginalTrackingNumber] nvarchar(max) NOT NULL,
    [CustomerName] nvarchar(max) NOT NULL,
    [CustomerPhone] nvarchar(max) NOT NULL,
    [Carrier] nvarchar(max) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [CancelReason] nvarchar(max) NULL,
    [Note] nvarchar(max) NULL,
    [ReturnAction] nvarchar(max) NULL,
    [EvidenceImagesJson] nvarchar(max) NULL,
    [RejectionReason] nvarchar(max) NULL,
    [InspectedAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ReturnRequest] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReturnRequest_Output_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Output] ([id]) ON DELETE CASCADE
);

CREATE TABLE [SupplierDebtLogImages] (
    [Id] int NOT NULL IDENTITY,
    [SupplierDebtLogId] int NOT NULL,
    [ImageUrl] nvarchar(2000) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SupplierDebtLogImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupplierDebtLogImages_SupplierDebtLog_SupplierDebtLogId] FOREIGN KEY ([SupplierDebtLogId]) REFERENCES [SupplierDebtLog] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [WarrantyClaim] (
    [Id] int NOT NULL IDENTITY,
    [ClaimNumber] nvarchar(50) NOT NULL,
    [VehicleId] int NOT NULL,
    [IssueDescription] nvarchar(MAX) NOT NULL,
    [MediaUrls] nvarchar(MAX) NULL,
    [ServiceCenterName] nvarchar(200) NULL,
    [ManufacturerClaimNumber] nvarchar(100) NULL,
    [Status] int NOT NULL,
    [ManufacturerDecision] nvarchar(MAX) NULL,
    [IsRecall] bit NOT NULL,
    [TotalPartsCost] decimal(18,2) NOT NULL,
    [TotalLaborCost] decimal(18,2) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_WarrantyClaim] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WarrantyClaim_Vehicle_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicle] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [WorkshopPayment] (
    [Id] int NOT NULL IDENTITY,
    [PaymentNumber] nvarchar(50) NOT NULL,
    [SourceType] nvarchar(30) NOT NULL,
    [SourceId] int NOT NULL,
    [CustomerName] nvarchar(100) NOT NULL,
    [CustomerPhone] nvarchar(20) NOT NULL,
    [VehicleInfo] nvarchar(200) NULL,
    [ServiceDescription] nvarchar(MAX) NULL,
    [SubTotal] decimal(18,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [PaymentMethod] nvarchar(30) NOT NULL,
    [PaymentStatus] nvarchar(30) NOT NULL,
    [ReceivedById] uniqueidentifier NULL,
    [PaidAt] datetimeoffset NULL,
    [Notes] nvarchar(MAX) NULL,
    [InvoicePrintedAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_WorkshopPayment] PRIMARY KEY ([Id])
);

CREATE TABLE [PurchaseInvoiceItem] (
    [Id] int NOT NULL IDENTITY,
    [PurchaseInvoiceId] int NOT NULL,
    [PurchaseRequestItemId] int NULL,
    [ProductVariantId] int NOT NULL,
    [ProductVariantColorId] int NULL,
    [ProductName] nvarchar(200) NULL,
    [VariantName] nvarchar(100) NULL,
    [ColorName] nvarchar(50) NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [TaxRate] decimal(5,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_PurchaseInvoiceItem] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchaseInvoiceItem_PurchaseInvoice_PurchaseInvoiceId] FOREIGN KEY ([PurchaseInvoiceId]) REFERENCES [PurchaseInvoice] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ReturnRequestItem] (
    [Id] int NOT NULL IDENTITY,
    [ReturnRequestId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductName] nvarchar(max) NOT NULL,
    [Sku] nvarchar(max) NOT NULL,
    [ThumbnailUrl] nvarchar(max) NULL,
    [Quantity] int NOT NULL,
    [ReturnQuantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ReturnRequestItem] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReturnRequestItem_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Product] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReturnRequestItem_ReturnRequest_ReturnRequestId] FOREIGN KEY ([ReturnRequestId]) REFERENCES [ReturnRequest] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [WarrantyClaimPart] (
    [Id] int NOT NULL IDENTITY,
    [WarrantyClaimId] int NOT NULL,
    [PartName] nvarchar(200) NOT NULL,
    [PartCode] nvarchar(100) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetimeoffset NULL,
    [UpdatedAt] datetimeoffset NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_WarrantyClaimPart] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WarrantyClaimPart_WarrantyClaim_WarrantyClaimId] FOREIGN KEY ([WarrantyClaimId]) REFERENCES [WarrantyClaim] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_SupportRequest_AssignedUserId] ON [SupportRequest] ([AssignedUserId]);

CREATE INDEX [IX_Output_LeadId] ON [Output] ([LeadId]);

CREATE INDEX [IX_MaintenanceHistory_TechnicianId] ON [MaintenanceHistory] ([TechnicianId]);

CREATE INDEX [IX_Invoice_UserId] ON [Invoice] ([UserId]);

CREATE INDEX [IX_PurchaseInvoice_PurchaseRequestId] ON [PurchaseInvoice] ([PurchaseRequestId]);

CREATE INDEX [IX_PurchaseInvoice_SupplierId] ON [PurchaseInvoice] ([SupplierId]);

CREATE INDEX [IX_PurchaseInvoiceItem_PurchaseInvoiceId] ON [PurchaseInvoiceItem] ([PurchaseInvoiceId]);

CREATE INDEX [IX_ReturnRequest_OrderId] ON [ReturnRequest] ([OrderId]);

CREATE INDEX [IX_ReturnRequestItem_ProductId] ON [ReturnRequestItem] ([ProductId]);

CREATE INDEX [IX_ReturnRequestItem_ReturnRequestId] ON [ReturnRequestItem] ([ReturnRequestId]);

CREATE INDEX [IX_SupplierDebtLogImages_SupplierDebtLogId] ON [SupplierDebtLogImages] ([SupplierDebtLogId]);

CREATE INDEX [IX_WarrantyClaim_VehicleId] ON [WarrantyClaim] ([VehicleId]);

CREATE INDEX [IX_WarrantyClaimPart_WarrantyClaimId] ON [WarrantyClaimPart] ([WarrantyClaimId]);

ALTER TABLE [MaintenanceHistory] ADD CONSTRAINT [FK_MaintenanceHistory_EmployeeProfile_TechnicianId] FOREIGN KEY ([TechnicianId]) REFERENCES [EmployeeProfile] ([Id]);

ALTER TABLE [Output] ADD CONSTRAINT [FK_Output_Lead_LeadId] FOREIGN KEY ([LeadId]) REFERENCES [Lead] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [PlateDossier] ADD CONSTRAINT [FK_PlateDossier_Output_OutputId] FOREIGN KEY ([OutputId]) REFERENCES [Output] ([id]);

ALTER TABLE [SupportRequest] ADD CONSTRAINT [FK_SupportRequest_Users_AssignedUserId] FOREIGN KEY ([AssignedUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260703140314_AddSalesAndWorkshopInvoicesAndWarranty', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Users] ADD [PasswordResetToken] nvarchar(max) NULL;

ALTER TABLE [Users] ADD [PasswordResetTokenExpiry] datetimeoffset NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260704133950_AddPasswordResetTokenFields', N'10.0.9');

COMMIT;
GO

