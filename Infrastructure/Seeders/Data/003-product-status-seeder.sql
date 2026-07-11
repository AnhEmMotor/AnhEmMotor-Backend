-- Source: ProductStatusSeeder
-- Seed product status values.

INSERT INTO "ProductStatuses" ("Key")
SELECT 'for-sale'
WHERE NOT EXISTS (SELECT 1 FROM "ProductStatuses" WHERE "Key" = 'for-sale');

INSERT INTO "ProductStatuses" ("Key")
SELECT 'out-of-business'
WHERE NOT EXISTS (SELECT 1 FROM "ProductStatuses" WHERE "Key" = 'out-of-business');
