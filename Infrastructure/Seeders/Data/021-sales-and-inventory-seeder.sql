-- Source: SalesAndInventorySeeder
-- Seed sales orders and contact records.

INSERT INTO "OutputOrders" ("CustomerName", "CustomerPhone", "CustomerAddress", "CreatedAt", "UpdatedAt", "StatusId", "PaymentStatus", "PaymentMethod", "DepositRatio", "LastStatusChangedAt")
SELECT 'Khách hàng 100', '0901234567', '1 Đường Láng, Hà Nội', CURRENT_TIMESTAMP - INTERVAL '1 day', CURRENT_TIMESTAMP - INTERVAL '1 day', 'completed', 'Paid', 'Banking', 10, CURRENT_TIMESTAMP - INTERVAL '1 day'
WHERE NOT EXISTS (SELECT 1 FROM "OutputOrders" WHERE "CustomerPhone" = '0901234567');
