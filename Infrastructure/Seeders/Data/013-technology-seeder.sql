-- Source: TechnologySeeder
-- Seed technology categories and sample technologies.

INSERT INTO "TechnologyCategories" ("Name")
SELECT 'An toàn'
WHERE NOT EXISTS (SELECT 1 FROM "TechnologyCategories" WHERE "Name" = 'An toàn');

INSERT INTO "TechnologyCategories" ("Name")
SELECT 'Động cơ & Vận hành'
WHERE NOT EXISTS (SELECT 1 FROM "TechnologyCategories" WHERE "Name" = 'Động cơ & Vận hành');

INSERT INTO "Technologies" ("Name", "DefaultTitle", "DefaultDescription", "CategoryId")
SELECT 'eSP+ (4 Van)', 'eSP+ (4 Van)', 'Động cơ thế hệ mới, chạy cực êm và tiết kiệm xăng.', tc."Id"
FROM "TechnologyCategories" tc
WHERE tc."Name" = 'Động cơ & Vận hành'
AND NOT EXISTS (SELECT 1 FROM "Technologies" WHERE "Name" = 'eSP+ (4 Van)');
