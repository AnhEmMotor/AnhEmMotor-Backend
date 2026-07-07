using System;
using System.Collections.Generic;

namespace Application.ApiContracts.Statistical.Responses;

public record InvoiceOverviewResponse(
    InvoiceOverviewKpi Kpi,
    List<InvoiceTrendData> TrendData,
    List<InvoiceProductData> ProductData,
    List<InvoicePaymentData> PaymentData,
    List<InvoiceListItem> InvoicesData
);

public record InvoiceOverviewKpi(
    decimal TotalInvoiced,
    decimal CollectedCash,
    decimal PendingTransit,
    decimal CanceledAmount
);

public record InvoiceTrendData(
    string Day,
    decimal OfflineRev,
    decimal OnlineRev
);

public record InvoiceProductData(
    string Name,
    decimal Value
);

public record InvoicePaymentData(
    string Name,
    decimal Value
);

public record InvoiceListItem(
    string Id,
    string Date,
    string Channel,
    string Category,
    string PaymentMethod,
    decimal Amount,
    string Status,
    InvoiceListItemDetails Details
);

public record InvoiceListItemDetails(
    string CustomerName,
    string Cccd,
    string ProductName,
    string Vin,
    string EngineNo,
    string ShippingProvider,
    string TrackingCode,
    List<InvoiceListItemPart> Items
);

public record InvoiceListItemPart(
    int Qty,
    string Name,
    string Sku
);
