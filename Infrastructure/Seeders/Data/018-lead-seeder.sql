-- Source: LeadSeeder
-- Seed sample leads.

INSERT INTO "Leads" ("FullName", "Email", "PhoneNumber", "Status", "Source", "Score", "InterestedVehicle", "CreatedAt", "UpdatedAt")
SELECT 'Nguyễn Văn Nam', 'nam.nguyen@gmail.com', '0987123456', 'New', 'WebStore', 30, 'Winner X 2024', TIMESTAMP '2026-01-01 00:00:00+00', TIMESTAMP '2026-01-01 00:00:00+00'
WHERE NOT EXISTS (SELECT 1 FROM "Leads" WHERE "Email" = 'nam.nguyen@gmail.com');
