-- Source: PermissionDataSeeder
-- Seed permission catalog.

INSERT INTO "Permissions" ("Name")
SELECT 'Permissions.Admin'
WHERE NOT EXISTS (SELECT 1 FROM "Permissions" WHERE "Name" = 'Permissions.Admin');

INSERT INTO "Permissions" ("Name")
SELECT 'Permissions.Marketing.LeadManagement.View'
WHERE NOT EXISTS (SELECT 1 FROM "Permissions" WHERE "Name" = 'Permissions.Marketing.LeadManagement.View');
