using System;
using System.Collections.Generic;

namespace Application.ApiContracts.PurchaseInvoice.Requests
{
    public class CreatePurchaseInvoiceRequest
    {
        public int? PurchaseOrderId { get; set; }

        public string? InvoiceNumber { get; set; }

        public DateTimeOffset InvoiceDate { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? DueDate { get; set; }

        public string? Note { get; set; }

        public List<CreatePurchaseInvoiceItemRequest> Items { get; set; } = [];
    }
}
