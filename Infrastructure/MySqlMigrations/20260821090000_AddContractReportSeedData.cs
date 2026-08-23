using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.MySqlMigrations
{
    public partial class AddContractReportSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO SalesContracts
(Id, ContractNumber, OutputId, CustomerId, ShowroomName, ShowroomTaxCode, ShowroomAddress, ShowroomRepresentative, CustomerFullName, CustomerCCCD, CustomerAddress, CustomerPhone, VehicleModel, VehicleVersion, VehicleColor, FrameNumber, EngineNumber, ActualSalePrice, DepositAmount, RemainingAmount, FinalPaymentDeadline, WarrantyPeriod, WarrantyScope, SpecialTerms, Status, SignedDate, ScannedFileUrl, Note, CreatedAt, UpdatedAt, DeletedAt)
SELECT v.Id, v.ContractNumber, NULL, NULL, 'Anh Em Motor - Chi nhánh trung tâm', '0109999999', 'TP. Hồ Chí Minh', 'Phòng Kinh doanh', v.CustomerFullName, v.CustomerCCCD, v.CustomerAddress, v.CustomerPhone, v.VehicleModel, v.VehicleVersion, v.VehicleColor, v.FrameNumber, v.EngineNumber, v.ActualSalePrice, v.DepositAmount, v.RemainingAmount, v.FinalPaymentDeadline, '36 tháng', 'Bảo hành chính hãng theo quy định nhà sản xuất', v.SpecialTerms, v.Status, v.SignedDate, NULL, 'Dữ liệu phục vụ kiểm tra báo cáo Accountant/contract', v.CreatedAt, v.CreatedAt, NULL
FROM (
SELECT '10000000-0000-0000-0000-000000000001' Id, 'HD-BX-2026-001' ContractNumber, 'Nguyễn Minh Anh' CustomerFullName, '079200000001' CustomerCCCD, 'TP. Hồ Chí Minh' CustomerAddress, '0901000001' CustomerPhone, 'Honda' VehicleModel, 'Winner X 2026' VehicleVersion, 'Đỏ đen' VehicleColor, 'FRM-BX-260001' FrameNumber, 'ENG-BX-260001' EngineNumber, 52000000.00 ActualSalePrice, 10000000.00 DepositAmount, 42000000.00 RemainingAmount, 1790787599000 FinalPaymentDeadline, 'Approved' Status, 1785897000000 SignedDate, 'Đã duyệt thanh toán đợt đầu' SpecialTerms, 1785897000000 CreatedAt
UNION ALL SELECT '10000000-0000-0000-0000-000000000002', 'HD-BX-2026-002', 'Trần Quốc Bảo', '079200000002', 'Bình Dương', '0901000002', 'Yamaha', 'NVX 155', 'Xám xanh', 'FRM-BX-260002', 'ENG-BX-260002', 58500000.00, 15000000.00, 43500000.00, 1792083599000, 'Signed', 1786518000000, 'Đã ký hợp đồng', 1786518000000
UNION ALL SELECT '10000000-0000-0000-0000-000000000003', 'HD-BX-2026-003', 'Lê Hoàng Nam', '079200000003', 'Đồng Nai', '0901000003', 'Vespa', 'Sprint 150', 'Trắng', 'FRM-BX-260003', 'ENG-BX-260003', 76000000.00, 20000000.00, 56000000.00, 1796057999000, 'PendingApproval', NULL, 'Chờ duyệt hồ sơ', 1787022900000
UNION ALL SELECT '10000000-0000-0000-0000-000000000004', 'HD-BX-2026-004', 'Phạm Thu Hà', '079200000004', 'Long An', '0901000004', 'Suzuki', 'Raider R150', 'Xanh đen', 'FRM-BX-260004', 'ENG-BX-260004', 49000000.00, 12000000.00, 37000000.00, 1797353999000, 'Fulfilled', 1787219100000, 'Đã hoàn tất giao xe', 1787219100000
) v
WHERE NOT EXISTS (SELECT 1 FROM SalesContracts existing WHERE existing.ContractNumber = v.ContractNumber);

INSERT INTO SupplierContracts
(Id, SupplierId, ContractNumber, ContractFilePath, EffectiveDate, ExpirationDate, ContractValue, Status, Terms, Note, CreditLimit, PaymentWindowDays, BankAccountNumber, BankName, MinimumVolumePerMonth, DiscountRate, ParentContractId, CreatedAt, UpdatedAt, DeletedAt)
SELECT v.Id, (SELECT Id FROM Supplier WHERE DeletedAt IS NULL ORDER BY Id LIMIT 1), v.ContractNumber, NULL, v.EffectiveDate, v.ExpirationDate, v.ContractValue, v.Status, v.Terms, 'Dữ liệu phục vụ kiểm tra báo cáo Accountant/contract', v.CreditLimit, v.PaymentWindowDays, v.BankAccountNumber, v.BankName, v.MinimumVolumePerMonth, v.DiscountRate, NULL, v.CreatedAt, v.CreatedAt, NULL
FROM (
SELECT '20000000-0000-0000-0000-000000000001' Id, 'HD-NCC-2026-001' ContractNumber, '2026-08-03 00:00:00' EffectiveDate, '2027-08-02 23:59:59' ExpirationDate, 185000000.00 ContractValue, 'Active' Status, 'Nhập phụ tùng định kỳ cho cửa hàng' Terms, 250000000.00 CreditLimit, 30 PaymentWindowDays, '012345678901' BankAccountNumber, 'Vietcombank' BankName, 100 MinimumVolumePerMonth, 5.00 DiscountRate, 1785720600000 CreatedAt
UNION ALL SELECT '20000000-0000-0000-0000-000000000002', 'HD-NCC-2026-002', '2026-08-08 00:00:00', '2027-02-07 23:59:59', 320000000.00, 'Completed', 'Cung cấp dầu nhớt và vật tư bảo dưỡng', 400000000.00, 45, '012345678902', 'BIDV', 150, 7.50, 1786158000000
UNION ALL SELECT '20000000-0000-0000-0000-000000000003', 'HD-NCC-2026-003', '2026-08-15 00:00:00', '2027-08-14 23:59:59', 275000000.00, 'PendingApproval', 'Cung cấp phụ kiện và đồ bảo hộ xe máy', 350000000.00, 30, '012345678903', 'ACB', 80, 4.50, 1786761900000
UNION ALL SELECT '20000000-0000-0000-0000-000000000004', 'HD-NCC-2026-004', '2026-08-20 00:00:00', '2026-12-31 23:59:59', 96000000.00, 'Draft', 'Hợp đồng thử nghiệm cho nhóm hàng tiêu hao', 120000000.00, 15, '012345678904', 'MB Bank', 50, 3.00, 1787214000000
) v
WHERE NOT EXISTS (SELECT 1 FROM SupplierContracts existing WHERE existing.ContractNumber = v.ContractNumber);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM SupplierContracts WHERE ContractNumber IN ('HD-NCC-2026-001', 'HD-NCC-2026-002', 'HD-NCC-2026-003', 'HD-NCC-2026-004');
DELETE FROM SalesContracts WHERE ContractNumber IN ('HD-BX-2026-001', 'HD-BX-2026-002', 'HD-BX-2026-003', 'HD-BX-2026-004');
");
        }
    }
}
