-- Source: ProductOptionSeeder
-- Seed product options and values.

INSERT INTO "Options" ("Name")
SELECT 'VehicleType'
WHERE NOT EXISTS (SELECT 1 FROM "Options" WHERE "Name" = 'VehicleType');

INSERT INTO "OptionValues" ("OptionId", "Name")
SELECT o."Id", 'Xe ga'
FROM "Options" o
WHERE o."Name" = 'VehicleType'
AND NOT EXISTS (
  SELECT 1 FROM "OptionValues" ov
  JOIN "Options" oo ON oo."Id" = ov."OptionId"
  WHERE oo."Name" = 'VehicleType' AND ov."Name" = 'Xe ga'
);
