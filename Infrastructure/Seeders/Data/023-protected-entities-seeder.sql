-- Source: ProtectedEntitiesSeeder
-- Seed protected roles and role assignments.

INSERT INTO "Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
SELECT '7d05a6f0-7d75-4f9f-9b6c-b1d2d1a1c001'::uuid, 'Admin', 'ADMIN', 'seed-role-admin'
WHERE NOT EXISTS (SELECT 1 FROM "Roles" WHERE "Name" = 'Admin');

INSERT INTO "Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "FullName", "Gender", "Status", "CreatedAt")
SELECT '2f39265b-74d0-4f26-82fe-3f2d08953c77'::uuid, 'example@example.com', 'EXAMPLE@EXAMPLE.COM', 'example@example.com', 'EXAMPLE@EXAMPLE.COM', TRUE, 'SET_BY_IDENTITY_HASH', 'Example Protected User', 'Male', 'Active', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Email" = 'example@example.com');
