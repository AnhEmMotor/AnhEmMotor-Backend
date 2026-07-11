SET NOCOUNT ON;
GO

-- Sample products and variants.
-- Assumption: catalog rows created in 001-sample-catalog.sql already exist.

IF NOT EXISTS (SELECT 1 FROM [Product] WHERE [Name] = N'Honda Vision 2024')
BEGIN
    SET IDENTITY_INSERT [Product] ON;
    INSERT INTO [Product] ([Id], [Name], [ShortDescription], [MetaTitle], [MetaDescription], [CategoryId], [StatusId], [BrandId], [Origin], [WarrantyPeriod], [Unit], [StdDot], [StdEce], [StdSnell], [StdJis], [Description]) VALUES
        (1, N'Honda Vision 2024', N'Tay ga đô thị tiết kiệm nhiên liệu, phù hợp đi làm và đi học.', N'Honda Vision 2024', N'Xe tay ga Honda Vision 2024 mới, thiết kế hiện đại, tiết kiệm xăng.', 1, N'for-sale', 1, N'Nhật Bản', N'24 tháng', N'chiếc', 1, 0, 0, 0, N'Phiên bản đi lại hàng ngày với động cơ eSP+.'),
        (2, N'Honda SH 150i 2024', N'Xe tay ga cao cấp, mạnh mẽ và sang trọng.', N'Honda SH 150i 2024', N'Xe tay ga cao cấp Honda SH 150i 2024 cho người yêu sự tiện nghi.', 1, N'for-sale', 1, N'Nhật Bản', N'36 tháng', N'chiếc', 1, 1, 0, 0, N'Phiên bản cao cấp dành cho người cần sự thoải mái và phong cách.'),
        (3, N'Yamaha Exciter 155 VVA', N'Xe số thể thao, bốc và nhanh.', N'Yamaha Exciter 155 VVA', N'Exciter 155 VVA với cảm giác lái mạnh mẽ và truyền thống.', 1, N'for-sale', 2, N'Nhật Bản', N'24 tháng', N'chiếc', 1, 0, 0, 0, N'Xe thể thao giá trị cho giới trẻ.'),
        (4, N'Yamaha Sirius', N'Xe số phổ thông, bền bỉ và kinh tế.', N'Yamaha Sirius', N'Yamaha Sirius dành cho nhu cầu đi lại hàng ngày.', 1, N'for-sale', 2, N'Nhật Bản', N'24 tháng', N'chiếc', 1, 0, 0, 0, N'Phiên bản kinh tế, tối ưu chi phí vận hành.'),
        (5, N'Suzuki Raider R150', N'Xe số côn tay mạnh mẽ, thiết kế gọn.', N'Suzuki Raider R150', N'Suzuki Raider R150 phù hợp người thích cảm giác lái thể thao.', 1, N'for-sale', 3, N'Nhật Bản', N'24 tháng', N'chiếc', 1, 0, 0, 0, N'Xe côn tay thể thao giá tốt.'),
        (6, N'Piaggio Liberty 125', N'Xe tay ga sang trọng và tiện dụng.', N'Piaggio Liberty 125', N'Piaggio Liberty 125 mang phong cách Italia cho đô thị.', 1, N'coming-soon', 4, N'Ý', N'36 tháng', N'chiếc', 1, 1, 0, 0, N'Phiên bản mới sắp ra mắt.'),
        (7, N'Kính chắn gió Honda Vision', N'Phụ kiện bảo vệ đầu xe.', N'Kính chắn gió Honda Vision', N'Kính chắn gió chính hãng cho Honda Vision.', 2, N'for-sale', 1, N'Nhật Bản', N'12 tháng', N'cái', 0, 0, 0, 0, N'Phụ kiện đi kèm cho xe ô tô điện?');
    SET IDENTITY_INSERT [Product] OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM [ProductVariant] pv JOIN [Product] p ON p.Id = pv.ProductId WHERE p.Name = N'Honda Vision 2024' AND pv.SKU = N'HON-VIS-2024-STD')
BEGIN
    SET IDENTITY_INSERT [ProductVariant] ON;
    INSERT INTO [ProductVariant] ([Id], [ProductId], [UrlSlug], [Price], [CoverImageUrl], [VariantName], [SKU], [Weight], [Dimensions], [Wheelbase], [SeatHeight], [GroundClearance], [FuelCapacity], [TireSize], [FrontBrake], [RearBrake], [FrontSuspension], [RearSuspension], [EngineType])
    SELECT 1, p.Id, N'honda-vision-2024-standard', 31100000, N'https://cdn.anhemmotor.com/variants/honda-vision-standard.png', N'Tiêu chuẩn', N'HON-VIS-2024-STD', 108.5, N'1,850 x 700 x 1,100', 1.27, 0.78, 0.14, 5.5, N'90/90-14', N'Đĩa', N'Tang trống', N'Ống lồng', N'Phuộc lò xo', N'4 thì, SOHC'
    FROM [Product] p WHERE p.Name = N'Honda Vision 2024';

    INSERT INTO [ProductVariant] ([Id], [ProductId], [UrlSlug], [Price], [CoverImageUrl], [VariantName], [SKU], [Weight], [Dimensions], [Wheelbase], [SeatHeight], [GroundClearance], [FuelCapacity], [TireSize], [FrontBrake], [RearBrake], [FrontSuspension], [RearSuspension], [EngineType])
    SELECT 2, p.Id, N'honda-vision-2024-special', 34500000, N'https://cdn.anhemmotor.com/variants/honda-vision-special.png', N'Đặc biệt', N'HON-VIS-2024-SPC', 109.2, N'1,850 x 700 x 1,100', 1.27, 0.78, 0.14, 5.5, N'90/90-14', N'Đĩa', N'Tang trống', N'Ống lồng', N'Phuộc lò xo', N'4 thì, SOHC'
    FROM [Product] p WHERE p.Name = N'Honda Vision 2024';

    INSERT INTO [ProductVariant] ([Id], [ProductId], [UrlSlug], [Price], [CoverImageUrl], [VariantName], [SKU], [Weight], [Dimensions], [Wheelbase], [SeatHeight], [GroundClearance], [FuelCapacity], [TireSize], [FrontBrake], [RearBrake], [FrontSuspension], [RearSuspension], [EngineType])
    SELECT 3, p.Id, N'honda-sh-150i-premium', 96000000, N'https://cdn.anhemmotor.com/variants/honda-sh-premium.png', N'Cao cấp', N'HON-SH-150I-PRM', 128.0, N'1,960 x 740 x 1,130', 1.31, 0.79, 0.16, 5.5, N'110/70-14', N'Đĩa', N'Tang trống', N'Phuộc ống lồng', N'Phuộc lò xo', N'4 thì, SOHC'
    FROM [Product] p WHERE p.Name = N'Honda SH 150i 2024';

    INSERT INTO [ProductVariant] ([Id], [ProductId], [UrlSlug], [Price], [CoverImageUrl], [VariantName], [SKU], [Weight], [Dimensions], [Wheelbase], [SeatHeight], [GroundClearance], [FuelCapacity], [TireSize], [FrontBrake], [RearBrake], [FrontSuspension], [RearSuspension], [EngineType])
    SELECT 4, p.Id, N'yamaha-exciter-155-standard', 48000000, N'https://cdn.anhemmotor.com/variants/yamaha-exciter-standard.png', N'Tiêu chuẩn', N'YAM-EXC-155-STD', 122.0, N'1,955 x 705 x 1,105', 1.33, 0.79, 0.15, 5.5, N'100/80-17', N'Đĩa', N'Tang trống', N'Phuộc trước ống lồng', N'Phuộc sau lò xo', N'4 thì, SOHC'
    FROM [Product] p WHERE p.Name = N'Yamaha Exciter 155 VVA';

    INSERT INTO [ProductVariant] ([Id], [ProductId], [UrlSlug], [Price], [CoverImageUrl], [VariantName], [SKU], [Weight], [Dimensions], [Wheelbase], [SeatHeight], [GroundClearance], [FuelCapacity], [TireSize], [FrontBrake], [RearBrake], [FrontSuspension], [RearSuspension], [EngineType])
    SELECT 5, p.Id, N'suzuki-raider-r150-standard', 50000000, N'https://cdn.anhemmotor.com/variants/suzuki-raider-standard.png', N'Tiêu chuẩn', N'SUZ-RAI-R150-STD', 121.5, N'1,945 x 710 x 1,090', 1.32, 0.79, 0.15, 4.8, N'90/90-17', N'Đĩa', N'Tang trống', N'Phuộc trước ống lồng', N'Phuộc sau lò xo', N'4 thì, SOHC'
    FROM [Product] p WHERE p.Name = N'Suzuki Raider R150';
    SET IDENTITY_INSERT [ProductVariant] OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM [ProductVariantColor] pvc JOIN [ProductVariant] pv ON pv.Id = pvc.ProductVariantId WHERE pv.SKU = N'HON-VIS-2024-STD' AND pvc.ColorName = N'Đỏ')
BEGIN
    SET IDENTITY_INSERT [ProductVariantColor] ON;
    INSERT INTO [ProductVariantColor] ([Id], [ProductVariantId], [ColorName], [ColorCode], [CoverImageUrl])
    SELECT 1, pv.Id, N'Đỏ', N'#d62828', N'https://cdn.anhemmotor.com/colors/red.png'
    FROM [ProductVariant] pv WHERE pv.SKU = N'HON-VIS-2024-STD';

    INSERT INTO [ProductVariantColor] ([Id], [ProductVariantId], [ColorName], [ColorCode], [CoverImageUrl])
    SELECT 2, pv.Id, N'Xanh', N'#2b6cb0', N'https://cdn.anhemmotor.com/colors/blue.png'
    FROM [ProductVariant] pv WHERE pv.SKU = N'HON-VIS-2024-SPC';

    INSERT INTO [ProductVariantColor] ([Id], [ProductVariantId], [ColorName], [ColorCode], [CoverImageUrl])
    SELECT 3, pv.Id, N'Trắng', N'#f5f5f5', N'https://cdn.anhemmotor.com/colors/white.png'
    FROM [ProductVariant] pv WHERE pv.SKU = N'HON-SH-150I-PRM';

    INSERT INTO [ProductVariantColor] ([Id], [ProductVariantId], [ColorName], [ColorCode], [CoverImageUrl])
    SELECT 4, pv.Id, N'Đen', N'#1f2937', N'https://cdn.anhemmotor.com/colors/black.png'
    FROM [ProductVariant] pv WHERE pv.SKU = N'YAM-EXC-155-STD';

    INSERT INTO [ProductVariantColor] ([Id], [ProductVariantId], [ColorName], [ColorCode], [CoverImageUrl])
    SELECT 5, pv.Id, N'Xám', N'#64748b', N'https://cdn.anhemmotor.com/colors/gray.png'
    FROM [ProductVariant] pv WHERE pv.SKU = N'SUZ-RAI-R150-STD';
    SET IDENTITY_INSERT [ProductVariantColor] OFF;
END
GO
