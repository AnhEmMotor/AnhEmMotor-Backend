-- Source: ProductCategorySeeder
-- Seed product categories.

INSERT INTO "ProductCategory" ("Name", "Slug", "IsActive", "ManagementType")
SELECT 'Xe máy', 'xe-may', TRUE, 'sku'
WHERE NOT EXISTS (SELECT 1 FROM "ProductCategory" WHERE "Name" = 'Xe máy');

INSERT INTO "ProductCategory" ("Name", "Slug", "IsActive", "ManagementType")
SELECT 'Phụ kiện', 'phu-kien', TRUE, 'sku'
WHERE NOT EXISTS (SELECT 1 FROM "ProductCategory" WHERE "Name" = 'Phụ kiện');
