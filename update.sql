BEGIN TRANSACTION;
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

CREATE TABLE [VoucherLeads] (
    [VoucherId] int NOT NULL,
    [LeadId] int NOT NULL,
    CONSTRAINT [PK_VoucherLeads] PRIMARY KEY ([VoucherId], [LeadId]),
    CONSTRAINT [FK_VoucherLeads_Lead_LeadId] FOREIGN KEY ([LeadId]) REFERENCES [Lead] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_VoucherLeads_Vouchers_VoucherId] FOREIGN KEY ([VoucherId]) REFERENCES [Vouchers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_VoucherLeads_LeadId] ON [VoucherLeads] ([LeadId]);

CREATE UNIQUE INDEX [IX_Vouchers_Code] ON [Vouchers] ([Code]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260706100056_AddVoucherTable', N'10.0.9');

COMMIT;
GO

