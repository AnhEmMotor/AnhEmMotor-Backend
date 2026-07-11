-- Source: ProductDataSeeder
-- Seed sample products and variants.

INSERT INTO "Products" ("Name", "ShortDescription", "CategoryId", "BrandId", "StatusId")
SELECT 'Honda Vision 2024', 'Dòng xe tay ga Honda Vision 2024', pc."Id", b."Id", 'for-sale'
FROM "ProductCategory" pc
JOIN "Brands" b ON b."Name" = 'Honda'
WHERE pc."Name" = 'Xe máy'
AND NOT EXISTS (SELECT 1 FROM "Products" WHERE "Name" = 'Honda Vision 2024');

INSERT INTO "ProductVariants" ("ProductId", "VariantName", "UrlSlug", "Price", "SKU")
SELECT p."Id", 'Tiêu chuẩn', 'honda-vision-2024-standard-red', 31100000, 'HO-VIS-2024-RED'
FROM "Products" p
WHERE p."Name" = 'Honda Vision 2024'
AND NOT EXISTS (SELECT 1 FROM "ProductVariants" pv JOIN "Products" pp ON pp."Id" = pv."ProductId" WHERE pp."Name" = 'Honda Vision 2024' AND pv."UrlSlug" = 'honda-vision-2024-standard-red');
