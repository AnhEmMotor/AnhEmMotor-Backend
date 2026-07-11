-- Source: CommissionPolicySeeder
-- Seed commission policies for categories.

INSERT INTO "CommissionPolicy" ("Name", "Type", "Value", "CategoryId", "EffectiveDate", "Unit", "Notes", "IsActive")
SELECT 'Hoa hồng Xe máy mặc định', 'FixedAmount', 500000, pc."Id", TIMESTAMP '2026-01-01 00:00:00+00', 'xe', 'Mức thưởng mặc định cho tất cả các dòng xe máy.', TRUE
FROM "ProductCategory" pc
WHERE pc."Name" = 'Xe máy'
AND NOT EXISTS (
  SELECT 1 FROM "CommissionPolicy" cp WHERE cp."CategoryId" = pc."Id" AND cp."Name" = 'Hoa hồng Xe máy mặc định'
);
