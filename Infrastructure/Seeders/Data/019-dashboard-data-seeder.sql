-- Source: DashboardDataSeeder
-- Seed dashboard sample rows.

INSERT INTO "Expenses" ("Name", "Amount", "ExpenseDate", "Category", "Note", "CreatedAt")
SELECT 'Tiền thuê mặt bằng tháng 6', 25000000, CURRENT_DATE, 'Fixed', 'Thuê showroom tháng 6/2026', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Expenses" WHERE "Name" = 'Tiền thuê mặt bằng tháng 6');
