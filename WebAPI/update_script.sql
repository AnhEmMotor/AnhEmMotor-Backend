BEGIN TRANSACTION;
ALTER TABLE [Product] ADD [DescriptionJson] nvarchar(max) NULL;

ALTER TABLE [Product] ADD [MetaDescriptionJson] nvarchar(max) NULL;

ALTER TABLE [Product] ADD [MetaTitleJson] nvarchar(max) NULL;

ALTER TABLE [Product] ADD [NameJson] nvarchar(max) NULL;

ALTER TABLE [Product] ADD [ShortDescriptionJson] nvarchar(max) NULL;

ALTER TABLE [Brand] ADD [DescriptionJson] nvarchar(max) NULL;

ALTER TABLE [Brand] ADD [NameJson] nvarchar(max) NULL;

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

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260708083146_AddJsonColumnsToProductAndBrand', N'10.0.9');

COMMIT;
GO

