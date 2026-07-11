-- Source: CarrierPartnerSeeder
-- Seed carrier partners.

INSERT INTO "CarrierPartners" ("CarrierCode", "Name", "IsActive", "Environment", "ApiBaseUrl", "ApiToken", "WebhookSecret", "WebhookEndpointUrl", "AutoSyncPricing", "MaxParcelWeightKg", "AllowLiquidCargo", "AllowOversizeCargo", "PricingRulesJson", "SlaJson", "CreatedAt")
SELECT 'ghtk', 'Giao Hàng Tiết Kiệm', TRUE, 'sandbox', 'https://services.giaohangtietkiem.vn/api/v1', 'demo-token-ghtk', 'demo-secret-ghtk', 'https://api.anhemmotor.com/v1/webhooks/ghtk', TRUE, 25, TRUE, FALSE, '[{"routeType":"IntraProvince","weightTier":"0-2kg","price":22000}]', '[{"routeType":"IntraProvince","expectedDays":"1-2 ngày"}]', CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "CarrierPartners" WHERE "CarrierCode" = 'ghtk');
