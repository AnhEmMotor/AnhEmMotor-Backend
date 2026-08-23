using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.PostgreSqlMigrations
{
    public partial class AddContractReportSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""SalesContracts"" (""Id"", ""ContractNumber"", ""OutputId"", ""CustomerId"", ""ShowroomName"", ""ShowroomTaxCode"", ""ShowroomAddress"", ""ShowroomRepresentative"", ""CustomerFullName"", ""CustomerCCCD"", ""CustomerAddress"", ""CustomerPhone"", ""VehicleModel"", ""VehicleVersion"", ""VehicleColor"", ""FrameNumber"", ""EngineNumber"", ""ActualSalePrice"", ""DepositAmount"", ""RemainingAmount"", ""FinalPaymentDeadline"", ""WarrantyPeriod"", ""WarrantyScope"", ""SpecialTerms"", ""Status"", ""SignedDate"", ""ScannedFileUrl"", ""Note"", ""CreatedAt"", ""UpdatedAt"", ""DeletedAt"")
SELECT v.""Id"", v.""ContractNumber"", NULL, NULL, 'Anh Em Motor - Chi nhánh trung tâm', '0109999999', 'TP. Hồ Chí Minh', 'Phòng Kinh doanh', v.""CustomerFullName"", v.""CustomerCCCD"", v.""CustomerAddress"", v.""CustomerPhone"", v.""VehicleModel"", v.""VehicleVersion"", v.""VehicleColor"", v.""FrameNumber"", v.""EngineNumber"", v.""ActualSalePrice"", v.""DepositAmount"", v.""RemainingAmount"", v.""FinalPaymentDeadline"", '36 tháng', 'Bảo hành chính hãng theo quy định nhà sản xuất', v.""SpecialTerms"", v.""Status"", v.""SignedDate"", NULL, 'Dữ liệu phục vụ kiểm tra báo cáo Accountant/contract', v.""CreatedAt"", v.""CreatedAt"", NULL
FROM (VALUES
('10000000-0000-0000-0000-000000000001'::uuid, 'HD-BX-2026-001', 'Nguyễn Minh Anh', '079200000001', 'TP. Hồ Chí Minh', '0901000001', 'Honda', 'Winner X 2026', 'Đỏ đen', 'FRM-BX-260001', 'ENG-BX-260001', 52000000.00::numeric, 10000000.00::numeric, 42000000.00::numeric, '2026-09-30T23:59:59+07:00'::timestamptz, 'Approved', '2026-08-05T09:30:00+07:00'::timestamptz, 'Đã duyệt thanh toán đợt đầu', '2026-08-05T09:30:00+07:00'::timestamptz),
('10000000-0000-0000-0000-000000000002'::uuid, 'HD-BX-2026-002', 'Trần Quốc Bảo', '079200000002', 'Bình Dương', '0901000002', 'Yamaha', 'NVX 155', 'Xám xanh', 'FRM-BX-260002', 'ENG-BX-260002', 58500000.00::numeric, 15000000.00::numeric, 43500000.00::numeric, '2026-10-15T23:59:59+07:00'::timestamptz, 'Signed', '2026-08-12T14:00:00+07:00'::timestamptz, 'Đã ký hợp đồng', '2026-08-12T14:00:00+07:00'::timestamptz),
('10000000-0000-0000-0000-000000000003'::uuid, 'HD-BX-2026-003', 'Lê Hoàng Nam', '079200000003', 'Đồng Nai', '0901000003', 'Vespa', 'Sprint 150', 'Trắng', 'FRM-BX-260003', 'ENG-BX-260003', 76000000.00::numeric, 20000000.00::numeric, 56000000.00::numeric, '2026-11-30T23:59:59+07:00'::timestamptz, 'PendingApproval', NULL, 'Chờ duyệt hồ sơ', '2026-08-18T10:15:00+07:00'::timestamptz),
('10000000-0000-0000-0000-000000000004'::uuid, 'HD-BX-2026-004', 'Phạm Thu Hà', '079200000004', 'Long An', '0901000004', 'Suzuki', 'Raider R150', 'Xanh đen', 'FRM-BX-260004', 'ENG-BX-260004', 49000000.00::numeric, 12000000.00::numeric, 37000000.00::numeric, '2026-12-15T23:59:59+07:00'::timestamptz, 'Fulfilled', '2026-08-20T16:45:00+07:00'::timestamptz, 'Đã hoàn tất giao xe', '2026-08-20T16:45:00+07:00'::timestamptz)
) AS v(""Id"", ""ContractNumber"", ""CustomerFullName"", ""CustomerCCCD"", ""CustomerAddress"", ""CustomerPhone"", ""VehicleModel"", ""VehicleVersion"", ""VehicleColor"", ""FrameNumber"", ""EngineNumber"", ""ActualSalePrice"", ""DepositAmount"", ""RemainingAmount"", ""FinalPaymentDeadline"", ""Status"", ""SignedDate"", ""SpecialTerms"", ""CreatedAt"")
WHERE NOT EXISTS (SELECT 1 FROM ""SalesContracts"" existing WHERE existing.""ContractNumber"" = v.""ContractNumber"");

INSERT INTO ""SupplierContracts"" (""Id"", ""SupplierId"", ""ContractNumber"", ""ContractFilePath"", ""EffectiveDate"", ""ExpirationDate"", ""ContractValue"", ""Status"", ""Terms"", ""Note"", ""CreditLimit"", ""PaymentWindowDays"", ""BankAccountNumber"", ""BankName"", ""MinimumVolumePerMonth"", ""DiscountRate"", ""ParentContractId"", ""CreatedAt"", ""UpdatedAt"", ""DeletedAt"")
SELECT v.""Id"", (SELECT ""Id"" FROM ""Supplier"" WHERE ""DeletedAt"" IS NULL ORDER BY ""Id"" LIMIT 1), v.""ContractNumber"", NULL, v.""EffectiveDate"", v.""ExpirationDate"", v.""ContractValue"", v.""Status"", v.""Terms"", 'Dữ liệu phục vụ kiểm tra báo cáo Accountant/contract', v.""CreditLimit"", v.""PaymentWindowDays"", v.""BankAccountNumber"", v.""BankName"", v.""MinimumVolumePerMonth"", v.""DiscountRate"", NULL, v.""CreatedAt"", v.""CreatedAt"", NULL
FROM (VALUES
('20000000-0000-0000-0000-000000000001'::uuid, 'HD-NCC-2026-001', '2026-08-03 00:00:00'::timestamp, '2027-08-02 23:59:59'::timestamp, 185000000.00::numeric, 'Active', 'Nhập phụ tùng định kỳ cho cửa hàng', 250000000.00::numeric, 30, '012345678901', 'Vietcombank', 100, 5.00::numeric, '2026-08-03 08:30:00'::timestamp),
('20000000-0000-0000-0000-000000000002'::uuid, 'HD-NCC-2026-002', '2026-08-08 00:00:00'::timestamp, '2027-02-07 23:59:59'::timestamp, 320000000.00::numeric, 'Completed', 'Cung cấp dầu nhớt và vật tư bảo dưỡng', 400000000.00::numeric, 45, '012345678902', 'BIDV', 150, 7.50::numeric, '2026-08-08 10:00:00'::timestamp),
('20000000-0000-0000-0000-000000000003'::uuid, 'HD-NCC-2026-003', '2026-08-15 00:00:00'::timestamp, '2027-08-14 23:59:59'::timestamp, 275000000.00::numeric, 'PendingApproval', 'Cung cấp phụ kiện và đồ bảo hộ xe máy', 350000000.00::numeric, 30, '012345678903', 'ACB', 80, 4.50::numeric, '2026-08-15 09:45:00'::timestamp),
('20000000-0000-0000-0000-000000000004'::uuid, 'HD-NCC-2026-004', '2026-08-20 00:00:00'::timestamp, '2026-12-31 23:59:59'::timestamp, 96000000.00::numeric, 'Draft', 'Hợp đồng thử nghiệm cho nhóm hàng tiêu hao', 120000000.00::numeric, 15, '012345678904', 'MB Bank', 50, 3.00::numeric, '2026-08-20 15:20:00'::timestamp)
) AS v(""Id"", ""ContractNumber"", ""EffectiveDate"", ""ExpirationDate"", ""ContractValue"", ""Status"", ""Terms"", ""CreditLimit"", ""PaymentWindowDays"", ""BankAccountNumber"", ""BankName"", ""MinimumVolumePerMonth"", ""DiscountRate"", ""CreatedAt"")
WHERE NOT EXISTS (SELECT 1 FROM ""SupplierContracts"" existing WHERE existing.""ContractNumber"" = v.""ContractNumber"");
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM ""SupplierContracts"" WHERE ""ContractNumber"" IN ('HD-NCC-2026-001', 'HD-NCC-2026-002', 'HD-NCC-2026-003', 'HD-NCC-2026-004');
DELETE FROM ""SalesContracts"" WHERE ""ContractNumber"" IN ('HD-BX-2026-001', 'HD-BX-2026-002', 'HD-BX-2026-003', 'HD-BX-2026-004');
");
        }
    }
}
