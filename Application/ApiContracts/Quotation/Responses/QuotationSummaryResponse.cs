using System;

namespace Application.ApiContracts.Quotation.Responses;

public class QuotationSummaryResponse
{
    public int? Id { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public int? ProductCount { get; set; }

    public string? Status { get; set; }

    public DateTimeOffset? LastUpdatedAt { get; set; }
}
