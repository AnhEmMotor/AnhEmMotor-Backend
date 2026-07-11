-- Source: SupplierContractSeeder
-- Seed supplier contracts.

INSERT INTO "SupplierContracts" ("Id", "SupplierId", "ContractNumber", "EffectiveDate", "ExpirationDate", "ContractValue", "Status", "Terms", "Note", "CreditLimit", "PaymentWindowDays", "BankAccountNumber", "BankName", "MinimumVolumePerMonth", "DiscountRate", "CreatedAt", "UpdatedAt")
SELECT 'b31dc30d-f0f4-4e5a-86e3-9f8d54a96a01'::uuid, s."Id", 'HD-HONDA-2024-001', DATE '2024-01-01', DATE '2025-12-31', 15000000000, 'active', 'Cung cấp xe máy chính hãng.', 'Hợp đồng mẫu cho nhà cung cấp Honda.', 5000000000, 30, '1234567890', 'Vietcombank', 100, 3.5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM "Suppliers" s
WHERE s."Name" LIKE '%Honda%'
AND NOT EXISTS (SELECT 1 FROM "SupplierContracts" WHERE "ContractNumber" = 'HD-HONDA-2024-001');
