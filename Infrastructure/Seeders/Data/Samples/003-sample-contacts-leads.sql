SET NOCOUNT ON;
GO

-- Sample contacts and leads.
-- Assumption: the Contact and Lead tables exist and support identity inserts.

IF NOT EXISTS (SELECT 1 FROM [Contact] WHERE [Email] = N'lan.nguyen@example.com')
BEGIN
    SET IDENTITY_INSERT [Contact] ON;
    INSERT INTO [Contact] ([Id], [FullName], [Email], [PhoneNumber], [Subject], [Message], [Status], [InternalNote], [Rating]) VALUES
        (1, N'Nguyễn Thị Lan', N'lan.nguyen@example.com', N'0901234567', N'Hỏi về bảo hành', N'Xin hỏi thời gian bảo hành cho xe Vision 2024 là bao lâu?', N'Answered', N'Gọi lại tư vấn sau 1 ngày.', 5),
        (2, N'Trần Văn Minh', N'minh.tran@example.com', N'0912345678', N'Đặt lịch test drive', N'Muốn đặt lịch test drive xe Exciter 155 vào tối thứ Bảy.', N'Pending', N'Chờ xác nhận lịch.', NULL),
        (3, N'Phạm Thị Hương', N'huong.pham@example.com', N'0923456789', N'Hỏi giá phụ kiện', N'Tôi cần báo giá kính chắn gió cho Honda Vision.', N'Closed', N'Đã gửi bảng giá.', 4),
        (4, N'Đỗ Quốc Anh', N'anh.do@example.com', N'0934567890', N'Yêu cầu tư vấn trả góp', N'Xin tư vấn gói trả góp cho xe SH 150i.', N'Pending', N'Đã gửi thông tin gói vay.', NULL),
        (5, N'Vũ Hồng Sơn', N'son.vu@example.com', N'0945678901', N'Khiếu nại', N'Xe nhận về có tiếng ồn ở phanh trước.', N'InProgress', N'Đã chuyển cho kỹ thuật.', 2);
    SET IDENTITY_INSERT [Contact] OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM [Lead] WHERE [Email] = N'lan.nguyen@example.com')
BEGIN
    SET IDENTITY_INSERT [Lead] ON;
    INSERT INTO [Lead] ([Id], [FullName], [Email], [PhoneNumber], [Score], [Status], [Source], [InterestedVehicle], [Notes], [Priority], [Address], [AddressDetail], [Ward], [District], [Province], [Gender], [Birthday], [IdentificationNumber], [Tier], [Points], [IsVerified], [AssignedToId]) VALUES
        (1, N'Nguyễn Thị Lan', N'lan.nguyen@example.com', N'0901234567', 82, N'New', N'WebStore', N'Honda Vision 2024', N'Khách hàng quan tâm xe đi làm.', N'High', N'Quận 7', N'123 Nguyễn Văn Linh', N'Tân Thuận', N'Quận 7', N'TP.HCM', N'Female', N'1994-05-10', N'271994000111', N'Gold', 240, 1, NULL),
        (2, N'Trần Văn Minh', N'minh.tran@example.com', N'0912345678', 74, N'Consulting', N'Facebook', N'Yamaha Exciter 155 VVA', N'Đang thảo luận về màu sắc và giá.', N'Medium', N'Bình Thạnh', N'45 Lê Văn Duyệt', N'Phường 1', N'Bình Thạnh', N'TP.HCM', N'Male', N'1992-08-21', N'271992000222', N'Silver', 105, 1, NULL),
        (3, N'Phạm Thị Hương', N'huong.pham@example.com', N'0923456789', 66, N'Hot', N'Shop', N'Suzuki Raider R150', N'Yêu cầu báo giá và giao xe trong tuần.', N'High', N'Biên Hòa', N'88 Nguyễn Ái Quốc', N'Tân Mai', N'Biên Hòa', N'Đồng Nai', N'Female', N'1998-02-15', N'361998000333', N'Gold', 320, 1, NULL),
        (4, N'Đỗ Quốc Anh', N'anh.do@example.com', N'0934567890', 91, N'FollowUp', N'CallCenter', N'Honda SH 150i 2024', N'Khách hàng có nhu cầu trả góp 24 tháng.', N'High', N'Long An', N'12 đường Bến Lức', N'Tân An', N'Long An', N'Long An', N'Male', N'1991-11-05', N'331991000444', N'Platinum', 520, 1, NULL),
        (5, N'Vũ Hồng Sơn', N'son.vu@example.com', N'0945678901', 58, N'New', N'WebStore', N'Piaggio Liberty 125', N'Quan tâm xe mới sắp ra mắt.', N'Medium', N'Hà Nội', N'20 Trần Phú', N'Ba Đình', N'Hà Nội', N'Hà Nội', N'Male', N'1997-04-30', N'001997000555', N'Silver', 92, 0, NULL);
    SET IDENTITY_INSERT [Lead] OFF;
END
GO
