-- ============================================================
-- Thêm 1 hợp đồng nhà cung cấp MỚI vào database AnhEmMotorDB
-- Chạy file này trong SQL Server Management Studio (SSMS)
-- hoặc bằng sqlcmd từ command line.
--
-- sqlcmd -S DESKTOP-HAIDVQD\SQLEXPRESS -d AnhEmMotorDB -i add_contract.sql
-- ============================================================

USE [AnhEmMotorDB];
GO

-- Xác định NCC Honda (lấy Id đầu tiên tên chứa "Honda")
DECLARE @SupplierId INT = (SELECT TOP 1 Id FROM Supplier WHERE Name LIKE N'%Honda%');
DECLARE @Now DATETIME = GETDATE();
DECLARE @NewContractId UNIQUEIDENTIFIER = NEWID();
DECLARE @ContractNumber NVARCHAR(100) = N'HD-HONDA-2025-001';

-- Kiểm tra trùng
IF EXISTS (SELECT 1 FROM SupplierContract WHERE ContractNumber = @ContractNumber)
BEGIN
    PRINT N'Hop dong ' + @ContractNumber + N' da ton tai. Bo qua.';
    RETURN;
END

IF @SupplierId IS NULL
BEGIN
    PRINT N'Khong tim thay nha cung cap Honda. Can tao supplier truoc.';
    RETURN;
END

-- ==== 1. Them hop dong ====
INSERT INTO SupplierContract (
    Id, SupplierId, ContractNumber, EffectiveDate, ExpirationDate,
    ContractValue, Status, Terms, Note,
    CreditLimit, PaymentWindowDays, BankAccountNumber, BankName,
    MinimumVolumePerMonth, DiscountRate, ContractFilePath,
    CreatedAt, UpdatedAt
)
VALUES (
    @NewContractId, @SupplierId, @ContractNumber,
    '2025-01-01', '2026-01-01',
    18000000000, N'Active',
    N'Hop dong cung cap xe Honda chinh hang nam 2025. Dieu khoan moi nhat ve gia si, thoi gian giao hang va bao hanh.',
    N'Hop dong moi nhat 2025 - gia uu dai dac biet.',
    6000000000, 30, N'1234567890', N'Ngan hang TMCP Ngoai thuong Viet Nam (Vietcombank)',
    120, 4.0, NULL,
    @Now, @Now
);

PRINT N'Da them hop dong: ' + @ContractNumber;

-- ==== 2. Them Audit Log ====
INSERT INTO SupplierContractAuditLog (Id, SupplierContractId, Action, Details, ChangedBy, IpAddress, OldValue, NewValue, CreatedAt)
VALUES (NEWID(), @NewContractId, N'Create', N'Created contract HD-HONDA-2025-001', N'manual-seed', NULL, NULL, @ContractNumber, @Now);

-- ==== 3. Them SKU Price List (4 variant Honda dau tien) ====
INSERT INTO SupplierContractItem (Id, SupplierContractId, ProductVariantId, WholesalePrice)
SELECT NEWID(), @NewContractId, v.Id, CAST(ISNULL(v.Price, 0) * 0.83 AS DECIMAL(18,2))
FROM (
    SELECT TOP 4 Id, Price, SKU
    FROM ProductVariant
    WHERE SKU LIKE 'HO%'
    ORDER BY Id
) v;

PRINT N'Da them ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' SKU items.';
PRINT N'Hoan tat! Contract ID: ' + CAST(@NewContractId AS NVARCHAR(50));
