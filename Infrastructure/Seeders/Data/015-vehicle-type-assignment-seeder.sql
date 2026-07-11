-- Source: VehicleTypeAssignmentSeeder
-- Seed vehicle type assignments for existing variants.

INSERT INTO "VariantOptionValues" ("VariantId", "OptionValueId")
SELECT pv."Id", ov."Id"
FROM "ProductVariants" pv
JOIN "Products" p ON p."Id" = pv."ProductId"
JOIN "OptionValues" ov ON ov."Name" = 'Xe ga'
JOIN "Options" o ON o."Id" = ov."OptionId" AND o."Name" = 'VehicleType'
WHERE p."Name" = 'Honda Vision 2024'
AND NOT EXISTS (
  SELECT 1 FROM "VariantOptionValues" vov WHERE vov."VariantId" = pv."Id" AND vov."OptionValueId" = ov."Id"
);
