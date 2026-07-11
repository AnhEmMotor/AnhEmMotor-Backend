-- Source: FinanceContractSeeder
-- Seed finance contracts.

INSERT INTO "FinanceContracts" ("Id", "ContractNumber", "CustomerId", "BankName", "LoanAmount", "TermMonths", "InterestRate", "DisbursementStatus", "CavetLocation", "SignedDate", "CreatedAt", "UpdatedAt")
SELECT 'b31dc30d-f0f4-4e5a-86e3-9f8d54a96a02'::uuid, 'TG-HDSAISON-2026-002', 'c18b55b9-a678-4a6f-bda3-1558a8625002'::uuid, 'HD Saison', 25000000, 12, 1.60, 'Pending', 'Bank', CURRENT_DATE + 2, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "FinanceContracts" WHERE "ContractNumber" = 'TG-HDSAISON-2026-002');
