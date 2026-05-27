using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Quotation.Responses;

public class QuotationDetailResponse
{
    public int? Id { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public List<QuotationItemResponse>? QuotationItems { get; set; }

    public DateTimeOffset? LastUpdatedAt { get; set; }
}
