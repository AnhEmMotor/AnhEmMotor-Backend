-- Source: LogisticsDataSeeder
-- Seed example parcel delivery orders.

INSERT INTO "ParcelDeliveryOrders" ("CustomerName", "CustomerPhone", "CustomerAddress", "Carrier", "TrackingNumber", "OriginalOrderCode", "Status", "CodAmount", "ShippingCost", "CreatedAt")
SELECT 'Nguyễn Văn A', '0987654321', '123 Đường Ba Tháng Hai, Quận 10, TP. Hồ Chí Minh', 'GHTK', '', 'ORD-2026-001', 'pending', 450000, 35000, CURRENT_TIMESTAMP - INTERVAL '12 hours'
WHERE NOT EXISTS (SELECT 1 FROM "ParcelDeliveryOrders" WHERE "OriginalOrderCode" = 'ORD-2026-001');
