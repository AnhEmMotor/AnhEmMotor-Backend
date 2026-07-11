SET NOCOUNT ON;
GO

-- Sample catalog data for Brand, ProductCategory and ProductStatus.
-- Assumption: these scripts run after the base seeder created the core status keys and the ProductCategory/Brand tables exist.

IF NOT EXISTS (SELECT 1 FROM [Brand] WHERE [Name] = N'Honda')
BEGIN
    SET IDENTITY_INSERT [Brand] ON;
    INSERT INTO [Brand] ([Id], [Name], [Origin], [LogoUrl], [Description]) VALUES
        (1, N'Honda', N'Nhật Bản', N'https://cdn.anhemmotor.com/brands/honda.png', N'Hãng xe máy Nhật Bản nổi tiếng với độ bền và tiết kiệm nhiên liệu.'),
        (2, N'Yamaha', N'Nhật Bản', N'https://cdn.anhemmotor.com/brands/yamaha.png', N'Thương hiệu xe máy thể thao và đô thị phổ biến tại Việt Nam.'),
        (3, N'Suzuki', N'Nhật Bản', N'https://cdn.anhemmotor.com/brands/suzuki.png', N'Nhà sản xuất xe máy linh hoạt, phù hợp nhiều đối tượng khách hàng.'),
        (4, N'Piaggio', N'Ý', N'https://cdn.anhemmotor.com/brands/piaggio.png', N'Xe máy phân khối lớn và xe tay ga cao cấp.' );
    SET IDENTITY_INSERT [Brand] OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM [ProductCategory] WHERE [Name] = N'Xe máy')
BEGIN
    SET IDENTITY_INSERT [ProductCategory] ON;
    INSERT INTO [ProductCategory] ([Id], [Name], [Slug], [ImageUrl], [IsActive], [Description], [ParentId], [MaxPurchaseQuantity], [ManagementType]) VALUES
        (1, N'Xe máy', N'xe-may', N'https://cdn.anhemmotor.com/categories/moto.png', 1, N'Các dòng xe máy tay ga, số và côn tay.', NULL, 5, N'sku'),
        (2, N'Phụ kiện', N'phu-kien', N'https://cdn.anhemmotor.com/categories/accessories.png', 1, N'Phụ kiện cho xe máy và bảo hộ.', NULL, 20, N'sku'),
        (3, N'Phụ tùng', N'phu-tung', N'https://cdn.anhemmotor.com/categories/spare-parts.png', 1, N'Phụ tùng thay thế cho xe máy.', NULL, 15, N'sku'),
        (4, N'Dịch vụ', N'dich-vu', N'https://cdn.anhemmotor.com/categories/services.png', 1, N'Dịch vụ bảo trì, đăng kiểm và lắp đặt.', NULL, 10, N'sku');
    SET IDENTITY_INSERT [ProductCategory] OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM [ProductStatus] WHERE [Key] = N'for-sale')
BEGIN
    INSERT INTO [ProductStatus] ([Key]) VALUES (N'for-sale');
END
GO

IF NOT EXISTS (SELECT 1 FROM [ProductStatus] WHERE [Key] = N'out-of-business')
BEGIN
    INSERT INTO [ProductStatus] ([Key]) VALUES (N'out-of-business');
END
GO

IF NOT EXISTS (SELECT 1 FROM [ProductStatus] WHERE [Key] = N'coming-soon')
BEGIN
    INSERT INTO [ProductStatus] ([Key]) VALUES (N'coming-soon');
END
GO
