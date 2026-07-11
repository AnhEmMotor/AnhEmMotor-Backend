-- Source: InventoryReceiptStatusSeeder
-- Seed inventory receipt status values.

INSERT INTO "InventoryReceiptStatuses" ("Key")
SELECT 'draft'
WHERE NOT EXISTS (SELECT 1 FROM "InventoryReceiptStatuses" WHERE "Key" = 'draft');

INSERT INTO "InventoryReceiptStatuses" ("Key")
SELECT 'approve'
WHERE NOT EXISTS (SELECT 1 FROM "InventoryReceiptStatuses" WHERE "Key" = 'approve');
