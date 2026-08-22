using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SqlServerMigrations
{
    public partial class AddContractReportSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO dbo.SalesContracts
(
    Id, ContractNumber, OutputId, CustomerId, ShowroomName, ShowroomTaxCode,
    ShowroomAddress, ShowroomRepresentative, CustomerFullName, CustomerCCCD,
    CustomerAddress, CustomerPhone, VehicleModel, VehicleVersion, VehicleColor,
    FrameNumber, EngineNumber, ActualSalePrice, DepositAmount, RemainingAmount,
    FinalPaymentDeadline, WarrantyPeriod, WarrantyScope, SpecialTerms, Status,
    SignedDate, ScannedFileUrl, Note, CreatedAt, UpdatedAt, DeletedAt
)
SELECT
    v.Id, v.ContractNumber, NULL, NULL, N'Anh Em Motor - Chi nhánh trung tâm',
    N'0109999999', N'TP. Hồ Chí Minh', N'Phòng Kinh doanh', v.CustomerFullName,
    v.CustomerCCCD, v.CustomerAddress, v.CustomerPhone, v.VehicleModel,
    v.VehicleVersion, v.VehicleColor, v.FrameNumber, v.EngineNumber,
    v.ActualSalePrice, v.DepositAmount, v.RemainingAmount, v.FinalPaymentDeadline,
    N'36 tháng', N'Bảo hành chính hãng theo quy định nhà sản xuất', v.SpecialTerms,
    v.Status, v.SignedDate, NULL,
    N'Dữ liệu phục vụ kiểm tra báo cáo Accountant/contract', v.CreatedAt, v.CreatedAt, NULL
FROM
(
    VALUES
    ('10000000-0000-0000-0000-000000000001', N'HD-BX-2026-001', N'Nguyễn Minh Anh', N'079200000001', N'TP. Hồ Chí Minh', N'0901000001', N'Honda', N'Winner X 2026', N'Đỏ đen', N'FRM-BX-260001', N'ENG-BX-260001', CAST(52000000.00 AS decimal(18,2)), CAST(10000000.00 AS decimal(18,2)), CAST(42000000.00 AS decimal(18,2)), CAST('2026-09-30T23:59:59+07:00' AS datetimeoffset), N'Approved', CAST('2026-08-05T09:30:00+07:00' AS datetimeoffset), N'Đã duyệt thanh toán đợt đầu', CAST('2026-08-05T09:30:00+07:00' AS datetimeoffset)),
    ('10000000-0000-0000-0000-000000000002', N'HD-BX-2026-002', N'Trần Quốc Bảo', N'079200000002', N'Bình Dương', N'0901000002', N'Yamaha', N'NVX 155', N'Xám xanh', N'FRM-BX-260002', N'ENG-BX-260002', CAST(58500000.00 AS decimal(18,2)), CAST(15000000.00 AS decimal(18,2)), CAST(43500000.00 AS decimal(18,2)), CAST('2026-10-15T23:59:59+07:00' AS datetimeoffset), N'Signed', CAST('2026-08-12T14:00:00+07:00' AS datetimeoffset), N'Đã ký hợp đồng', CAST('2026-08-12T14:00:00+07:00' AS datetimeoffset)),
    ('10000000-0000-0000-0000-000000000003', N'HD-BX-2026-003', N'Lê Hoàng Nam', N'079200000003', N'Đồng Nai', N'0901000003', N'Vespa', N'Sprint 150', N'Trắng', N'FRM-BX-260003', N'ENG-BX-260003', CAST(76000000.00 AS decimal(18,2)), CAST(20000000.00 AS decimal(18,2)), CAST(56000000.00 AS decimal(18,2)), CAST('2026-11-30T23:59:59+07:00' AS datetimeoffset), N'PendingApproval', NULL, N'Chờ duyệt hồ sơ', CAST('2026-08-18T10:15:00+07:00' AS datetimeoffset)),
    ('10000000-0000-0000-0000-000000000004', N'HD-BX-2026-004', N'Phạm Thu Hà', N'079200000004', N'Long An', N'0901000004', N'Suzuki', N'Raider R150', N'Xanh đen', N'FRM-BX-260004', N'ENG-BX-260004', CAST(49000000.00 AS decimal(18,2)), CAST(12000000.00 AS decimal(18,2)), CAST(37000000.00 AS decimal(18,2)), CAST('2026-12-15T23:59:59+07:00' AS datetimeoffset), N'Fulfilled', CAST('2026-08-20T16:45:00+07:00' AS datetimeoffset), N'Đã hoàn tất giao xe', CAST('2026-08-20T16:45:00+07:00' AS datetimeoffset))
) AS v
(
    Id, ContractNumber, CustomerFullName, CustomerCCCD, CustomerAddress, CustomerPhone,
    VehicleModel, VehicleVersion, VehicleColor, FrameNumber, EngineNumber,
    ActualSalePrice, DepositAmount, RemainingAmount, FinalPaymentDeadline,
    Status, SignedDate, SpecialTerms, CreatedAt
)
WHERE NOT EXISTS (SELECT 1 FROM dbo.SalesContracts existing WHERE existing.ContractNumber = v.ContractNumber);

INSERT INTO dbo.SupplierContracts
(
    Id, SupplierId, ContractNumber, ContractFilePath, EffectiveDate, ExpirationDate,
    ContractValue, Status, Terms, Note, CreditLimit, PaymentWindowDays,
    BankAccountNumber, BankName, MinimumVolumePerMonth, DiscountRate,
    ParentContractId, CreatedAt, UpdatedAt, DeletedAt
)
SELECT
    v.Id, (SELECT TOP (1) Id FROM dbo.Supplier WHERE DeletedAt IS NULL ORDER BY Id),
    v.ContractNumber, NULL, v.EffectiveDate, v.ExpirationDate, v.ContractValue,
    v.Status, v.Terms, N'Dữ liệu phục vụ kiểm tra báo cáo Accountant/contract',
    v.CreditLimit, v.PaymentWindowDays, v.BankAccountNumber, v.BankName,
    v.MinimumVolumePerMonth, v.DiscountRate, NULL, v.CreatedAt, v.CreatedAt, NULL
FROM
(
    VALUES
    ('20000000-0000-0000-0000-000000000001', N'HD-NCC-2026-001', CAST('2026-08-03T00:00:00' AS datetime2), CAST('2027-08-02T23:59:59' AS datetime2), CAST(185000000.00 AS decimal(18,2)), N'Active', N'Nhập phụ tùng định kỳ cho cửa hàng', CAST(250000000.00 AS decimal(18,2)), 30, N'012345678901', N'Vietcombank', 100, CAST(5.00 AS decimal(5,2)), CAST('2026-08-03T08:30:00+07:00' AS datetimeoffset)),
    ('20000000-0000-0000-0000-000000000002', N'HD-NCC-2026-002', CAST('2026-08-08T00:00:00' AS datetime2), CAST('2027-02-07T23:59:59' AS datetime2), CAST(320000000.00 AS decimal(18,2)), N'Completed', N'Cung cấp dầu nhớt và vật tư bảo dưỡng', CAST(400000000.00 AS decimal(18,2)), 45, N'012345678902', N'BIDV', 150, CAST(7.50 AS decimal(5,2)), CAST('2026-08-08T10:00:00+07:00' AS datetimeoffset)),
    ('20000000-0000-0000-0000-000000000003', N'HD-NCC-2026-003', CAST('2026-08-15T00:00:00' AS datetime2), CAST('2027-08-14T23:59:59' AS datetime2), CAST(275000000.00 AS decimal(18,2)), N'PendingApproval', N'Cung cấp phụ kiện và đồ bảo hộ xe máy', CAST(350000000.00 AS decimal(18,2)), 30, N'012345678903', N'ACB', 80, CAST(4.50 AS decimal(5,2)), CAST('2026-08-15T09:45:00+07:00' AS datetimeoffset)),
    ('20000000-0000-0000-0000-000000000004', N'HD-NCC-2026-004', CAST('2026-08-20T00:00:00' AS datetime2), CAST('2026-12-31T23:59:59' AS datetime2), CAST(96000000.00 AS decimal(18,2)), N'Draft', N'Hợp đồng thử nghiệm cho nhóm hàng tiêu hao', CAST(120000000.00 AS decimal(18,2)), 15, N'012345678904', N'MB Bank', 50, CAST(3.00 AS decimal(5,2)), CAST('2026-08-20T15:20:00+07:00' AS datetimeoffset))
) AS v
(Id, ContractNumber, EffectiveDate, ExpirationDate, ContractValue, Status, Terms, CreditLimit, PaymentWindowDays, BankAccountNumber, BankName, MinimumVolumePerMonth, DiscountRate, CreatedAt)
WHERE NOT EXISTS (SELECT 1 FROM dbo.SupplierContracts existing WHERE existing.ContractNumber = v.ContractNumber);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM dbo.SupplierContracts WHERE ContractNumber IN (N'HD-NCC-2026-001', N'HD-NCC-2026-002', N'HD-NCC-2026-003', N'HD-NCC-2026-004');
DELETE FROM dbo.SalesContracts WHERE ContractNumber IN (N'HD-BX-2026-001', N'HD-BX-2026-002', N'HD-BX-2026-003', N'HD-BX-2026-004');
");
        }
    }
}
