-- Source: NewsCategorySeeder
-- Seed news categories.

INSERT INTO "NewsCategories" ("Id", "Name", "Slug", "IsActive", "CreatedAt")
SELECT 1, 'Tư vấn mua xe', 'tu-van-mua-xe', TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "NewsCategories" WHERE "Id" = 1);
