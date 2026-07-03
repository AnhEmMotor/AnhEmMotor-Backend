-- ============================================================================
-- sp_LogisticsDashboard
-- Dashboard thống kê phân hệ vận chuyển (xưởng)
-- Trả về 5 result sets: Summary, Funnel, Trends, CarrierScorecard, Exceptions
-- ============================================================================
CREATE OR ALTER PROCEDURE [dbo].[sp_LogisticsDashboard]
    @FromDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- ========================================================================
    -- 1. SUMMARY CARDS (1 hàng)
    -- ========================================================================
    SELECT
        -- FulfillmentWorkload = Pending + Packing
        SUM(CASE WHEN Status IN (0, 1) THEN 1 ELSE 0 END) AS FulfillmentWorkload,

        -- Overload flag (1 nếu workload > 50)
        CASE WHEN SUM(CASE WHEN Status IN (0, 1) THEN 1 ELSE 0 END) > 50 THEN 1 ELSE 0 END AS FulfillmentWorkloadIsOverload,

        -- PendingUnreconciledCod = tổng CodAmount của đơn đang Shipping
        ISNULL(SUM(CASE WHEN Status = 2 THEN CodAmount ELSE 0 END), 0) AS PendingUnreconciledCod,

        -- OTIF Rate: tỷ lệ giao đúng hạn / tổng đã giao
        CASE
            WHEN SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END) > 0
            THEN CAST(SUM(CASE WHEN Status = 3 AND DeliveredAt IS NOT NULL AND ExpectedAt IS NOT NULL AND DeliveredAt <= ExpectedAt THEN 1 ELSE 0 END) AS FLOAT)
                 / SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END)
            ELSE 0.0
        END AS OtifRate,

        -- ReturnsClaimsRate: tỷ lệ hoàn trả / tổng đơn đã gửi
        CASE
            WHEN COUNT(*) > 0
            THEN CAST(SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*)
            ELSE 0.0
        END AS ReturnsClaimsRate
    FROM ParcelDeliveryOrders
    WHERE CreatedAt >= @FromDate;

    -- ========================================================================
    -- 2. FULFILLMENT FUNNEL (số lượng theo trạng thái)
    -- ========================================================================
    SELECT
        StatusName = CASE Status
            WHEN 0 THEN 'Pending'
            WHEN 1 THEN 'Packing'
            WHEN 2 THEN 'Shipping'
            WHEN 3 THEN 'Completed'
            WHEN 4 THEN 'Returned'
            ELSE CAST(Status AS VARCHAR(20))
        END,
        [Count] = COUNT(*)
    FROM ParcelDeliveryOrders
    WHERE CreatedAt >= @FromDate
    GROUP BY Status;

    -- ========================================================================
    -- 3. TRENDS (14 ngày giao hàng gần nhất)
    -- ========================================================================
    SELECT TOP 14
        DayLabel = FORMAT(DeliveredAt, 'dd/MM'),
        DeliveredCount = COUNT(*),
        ShippingCost = SUM(ShippingCost)
    FROM ParcelDeliveryOrders
    WHERE Status = 3
      AND DeliveredAt IS NOT NULL
      AND DeliveredAt >= @FromDate
    GROUP BY DeliveredAt
    ORDER BY DeliveredAt ASC;

    -- ========================================================================
    -- 4. CARRIER SCORECARD
    -- ========================================================================
    SELECT
        Carrier = Carrier,
        DeliveredCount = COUNT(*),
        AvgDeliveryDays = AVG(CAST(DATEDIFF(DAY, CreatedAt, DeliveredAt) AS FLOAT)),
        AvgShippingCostPerOrder = AVG(ShippingCost),
        ReturnsRatio = CAST(cr.ReturnedCount AS FLOAT) / COUNT(*)
    FROM ParcelDeliveryOrders pdo
    OUTER APPLY (
        SELECT COUNT(*) AS ReturnedCount
        FROM ParcelDeliveryOrders r
        WHERE r.Carrier = pdo.Carrier
          AND r.Status = 4
          AND r.CreatedAt >= @FromDate
    ) cr
    WHERE pdo.Status = 3
      AND pdo.DeliveredAt IS NOT NULL
      AND pdo.CreatedAt >= @FromDate
    GROUP BY pdo.Carrier, cr.ReturnedCount
    ORDER BY DeliveredCount DESC;

    -- ========================================================================
    -- 5. EXCEPTIONS (top 20 mỗi loại)
    -- ========================================================================

    -- 5a. Ngâm kho: Pending > 24h chưa chuyển Packing
    SELECT TOP 20
        Type = 'ngam_kho',
        TrackingNumber = TrackingNumber,
        Message = N'Đơn pending quá 24h mà chưa chuyển trạng thái đóng gói.',
        CreatedAt = CreatedAt
    FROM ParcelDeliveryOrders
    WHERE Status = 0
      AND DATEDIFF(HOUR, CreatedAt, SYSUTCDATETIME()) > 24
      AND CreatedAt >= @FromDate
    ORDER BY CreatedAt ASC;

    -- 5b. Giao chậm: Shipping > 4 ngày chưa Completed
    SELECT TOP 20
        Type = 'giao_cham',
        TrackingNumber = TrackingNumber,
        Message = N'Đơn đang shipping quá 4 ngày chưa cập nhật Completed.',
        CreatedAt = CreatedAt
    FROM ParcelDeliveryOrders
    WHERE Status = 2
      AND DATEDIFF(DAY, CreatedAt, SYSUTCDATETIME()) > 4
      AND CreatedAt >= @FromDate
    ORDER BY CreatedAt ASC;

    -- 5c. Hoàn chờ kiểm tra: Returned nhưng InspectedAt IS NULL
    SELECT TOP 20
        Type = 'hoan_cho_kiem_tra',
        TrackingNumber = TrackingNumber,
        Message = N'Hàng hoàn đã về nhưng chưa khui hộp/duyệt nhập lại kho.',
        CreatedAt = CreatedAt
    FROM ParcelDeliveryOrders
    WHERE Status = 4
      AND InspectedAt IS NULL
      AND CreatedAt >= @FromDate
    ORDER BY CreatedAt ASC;
END;
GO
