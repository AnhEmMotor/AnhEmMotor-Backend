-- Source: OutputStatusSeeder
-- Seed output status values.

INSERT INTO "OutputStatuses" ("Key")
SELECT 'pending'
WHERE NOT EXISTS (SELECT 1 FROM "OutputStatuses" WHERE "Key" = 'pending');

INSERT INTO "OutputStatuses" ("Key")
SELECT 'completed'
WHERE NOT EXISTS (SELECT 1 FROM "OutputStatuses" WHERE "Key" = 'completed');
