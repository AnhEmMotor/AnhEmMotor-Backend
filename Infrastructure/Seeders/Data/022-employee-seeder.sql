-- Source: EmployeeSeeder
-- Seed employee users and profiles.

INSERT INTO "Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "FullName", "Gender", "Status", "CreatedAt")
SELECT '9b1d2d32-8c8d-4b3f-a1c1-9cf2a6d4c101'::uuid, 'nguyen.van.a@anhemmotor.com', 'NGUYEN.VAN.A@ANHEMMOTOR.COM', 'nguyen.van.a@anhemmotor.com', 'NGUYEN.VAN.A@ANHEMMOTOR.COM', TRUE, 'SET_BY_IDENTITY_HASH', 'Nguyễn Văn A', 'Male', 'Active', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Email" = 'nguyen.van.a@anhemmotor.com');

INSERT INTO "EmployeeProfiles" ("UserId", "JobTitle", "BaseSalary", "IdentityNumber", "Address", "ContractDate", "BankName", "BankAccountNumber", "CreatedAt")
SELECT '9b1d2d32-8c8d-4b3f-a1c1-9cf2a6d4c101'::uuid, 'Trưởng phòng Kinh doanh', 25000000, '03112345678', 'Biên Hòa, Đồng Nai', DATE '2024-01-01', 'Vietcombank', '1011234567', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "EmployeeProfiles" WHERE "UserId" = '9b1d2d32-8c8d-4b3f-a1c1-9cf2a6d4c101'::uuid);
