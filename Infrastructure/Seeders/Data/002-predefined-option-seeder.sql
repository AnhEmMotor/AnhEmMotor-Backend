-- Source: PredefinedOptionSeeder
-- Seed reusable option labels.

INSERT INTO "PredefinedOptions" ("Key", "Value")
SELECT 'VehicleType', 'Loại xe'
WHERE NOT EXISTS (SELECT 1 FROM "PredefinedOptions" WHERE "Key" = 'VehicleType');

INSERT INTO "PredefinedOptions" ("Key", "Value")
SELECT 'Displacement', 'Phân khối'
WHERE NOT EXISTS (SELECT 1 FROM "PredefinedOptions" WHERE "Key" = 'Displacement');
