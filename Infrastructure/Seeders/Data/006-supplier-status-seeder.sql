-- Source: SupplierStatusSeeder
-- Seed supplier status values.

INSERT INTO "SupplierStatuses" ("Key")
SELECT 'active'
WHERE NOT EXISTS (SELECT 1 FROM "SupplierStatuses" WHERE "Key" = 'active');

INSERT INTO "SupplierStatuses" ("Key")
SELECT 'inactive'
WHERE NOT EXISTS (SELECT 1 FROM "SupplierStatuses" WHERE "Key" = 'inactive');
