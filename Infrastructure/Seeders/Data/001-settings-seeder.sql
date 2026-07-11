-- Source: SettingsSeeder
-- Seed default system settings.

INSERT INTO "Settings" ("Key", "Value")
SELECT 'InventoryAlertLevel', '5'
WHERE NOT EXISTS (SELECT 1 FROM "Settings" WHERE "Key" = 'InventoryAlertLevel');

INSERT INTO "Settings" ("Key", "Value")
SELECT 'DepositRatio', '50'
WHERE NOT EXISTS (SELECT 1 FROM "Settings" WHERE "Key" = 'DepositRatio');
