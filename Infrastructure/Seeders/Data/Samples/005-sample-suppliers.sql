SET NOCOUNT ON;
GO

-- Sample suppliers and partner type/status references.
-- Assumption: SupplierStatus and PartnerType tables exist with relevant keys.

IF NOT EXISTS (SELECT 1 FROM [SupplierStatus] WHERE [Key] = N'active')
BEGIN
    INSERT INTO [SupplierStatus] ([Key]) VALUES (N'active');
END
GO

IF NOT EXISTS (SELECT 1 FROM [PartnerType] WHERE [Key] = N'principal')
BEGIN
    INSERT INTO [PartnerType] ([Key]) VALUES (N'principal');
END
GO

IF NOT EXISTS (SELECT 1 FROM [Supplier] WHERE [Email] = N'sales@honda-vn.com')
BEGIN
    SET IDENTITY_INSERT [Supplier] ON;
    INSERT INTO [Supplier] ([Id], [Name], [Phone], [Email], [StatusId], [Notes], [Address], [TaxIdentificationNumber], [PartnerTypeId], [CreatedAt], [UpdatedAt]) VALUES
        (1, N'Honda Việt Nam', N'02838123456', N'sales@honda-vn.com', N'active', N'Nhà cung cấp chính thức cho dòng xe Vision và SH.', N'123 Nguyễn Văn Linh, Quận 7, TP.HCM', N'0301234567', N'principal', N'2026-07-01T09:00:00+07:00', N'2026-07-01T09:00:00+07:00'),
        (2, N'Yamaha Motor Việt Nam', N'02838123457', N'supply@yamaha-vn.com', N'active', N'Nhà phân phối xe thể thao và xe đô thị.', N'456 Hai Bà Trưng, Quận 1, TP.HCM', N'0301234568', N'principal', N'2026-07-02T09:00:00+07:00', N'2026-07-02T09:00:00+07:00'),
        (3, N'Suzuki Việt Nam', N'02838123458', N'procurement@suzuki-vn.com', N'active', N'Nhà cung cấp phụ tùng và xe côn tay.', N'789 Trần Hưng Đạo, Quận 5, TP.HCM', N'0301234569', N'principal', N'2026-07-03T09:00:00+07:00', N'2026-07-03T09:00:00+07:00');
    SET IDENTITY_INSERT [Supplier] OFF;
END
GO
