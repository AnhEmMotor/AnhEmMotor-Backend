SET NOCOUNT ON;
GO

-- Sample bookings and vehicles referencing the products and leads created earlier.
-- Assumption: ProductVariant and Lead tables already contain the rows inserted by previous scripts.

IF NOT EXISTS (SELECT 1 FROM [Booking] WHERE [Email] = N'booking1@example.com')
BEGIN
    SET IDENTITY_INSERT [Booking] ON;
    INSERT INTO [Booking] ([Id], [FullName], [Email], [PhoneNumber], [ProductVariantId], [PreferredDate], [Note], [Status], [BookingType], [Location], [CreatedAt], [UpdatedAt]) VALUES
        (1, N'Nguyễn Thị Lan', N'booking1@example.com', N'0901234567', 1, N'2026-08-10T09:00:00+07:00', N'Khách hàng muốn test drive xe Vision vào sáng cuối tuần.', N'Pending', N'TestDrive', N'Showroom Quận 7', N'2026-07-01T09:00:00+07:00', N'2026-07-01T09:00:00+07:00'),
        (2, N'Trần Văn Minh', N'booking2@example.com', N'0912345678', 4, N'2026-08-12T14:00:00+07:00', N'Yêu cầu xem xe Exciter màu đen.', N'Confirmed', N'TestDrive', N'Showroom Bình Thạnh', N'2026-07-02T10:30:00+07:00', N'2026-07-02T10:30:00+07:00'),
        (3, N'Phạm Thị Hương', N'booking3@example.com', N'0923456789', 5, N'2026-08-15T16:00:00+07:00', N'Đặt lịch xem xe Raider R150.', N'Completed', N'Purchase', N'Showroom Biên Hòa', N'2026-07-03T11:15:00+07:00', N'2026-07-03T11:15:00+07:00'),
        (4, N'Đỗ Quốc Anh', N'booking4@example.com', N'0934567890', 3, N'2026-08-18T10:00:00+07:00', N'Muốn xem SH 150i trước khi quyết định mua.', N'Cancelled', N'TestDrive', N'Showroom Long An', N'2026-07-04T13:45:00+07:00', N'2026-07-04T13:45:00+07:00');
    SET IDENTITY_INSERT [Booking] OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM [Vehicle] WHERE [VinNumber] = N'VIN-001-2026')
BEGIN
    SET IDENTITY_INSERT [Vehicle] ON;
    INSERT INTO [Vehicle] ([Id], [LeadId], [UserId], [InventoryReceiptInfoId], [OutputInfoId], [ProductId], [ProductVariantId], [ProductVariantColorId], [VinNumber], [EngineNumber], [LicensePlate], [CurrentOdo], [LastMaintenanceDate], [NextMaintenanceDate], [NextMaintenanceOdo], [ElectronicWarrantyQrCode], [IsActive], [Status], [PurchaseDate], [ImportPrice], [CreatedAt], [UpdatedAt]) VALUES
        (1, 1, NULL, NULL, NULL, 1, 1, 1, N'VIN-001-2026', N'ENG-001-2026', N'51A-12345', 1250, N'2026-06-01', N'2026-09-01', 3000, N'QR-VIS-001', 1, N'Available', N'2026-07-01T00:00:00+07:00', 31100000, N'2026-07-01T00:00:00+07:00', N'2026-07-01T00:00:00+07:00'),
        (2, 2, NULL, NULL, NULL, 4, 4, 4, N'VIN-002-2026', N'ENG-002-2026', N'51B-67890', 980, N'2026-05-20', N'2026-08-20', 2500, N'QR-EXC-002', 1, N'Sold', N'2026-07-02T00:00:00+07:00', 48000000, N'2026-07-02T00:00:00+07:00', N'2026-07-02T00:00:00+07:00'),
        (3, 3, NULL, NULL, NULL, 5, 5, 5, N'VIN-003-2026', N'ENG-003-2026', N'51C-24680', 760, N'2026-04-15', N'2026-07-15', 2000, N'QR-RAI-003', 1, N'Available', N'2026-07-03T00:00:00+07:00', 50000000, N'2026-07-03T00:00:00+07:00', N'2026-07-03T00:00:00+07:00');
    SET IDENTITY_INSERT [Vehicle] OFF;
END
GO
