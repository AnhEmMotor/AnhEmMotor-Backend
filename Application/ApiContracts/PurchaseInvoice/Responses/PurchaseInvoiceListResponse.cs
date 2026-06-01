using System;

namespace Application.ApiContracts.PurchaseInvoice.Responses
{
    public class PurchaseInvoiceListResponse
    {
        public int Id { get; set; }

        public int? PurchaseOrderId { get; set; }

        public string? InvoiceNumber { get; set; }

        public DateTimeOffset InvoiceDate { get; set; }

        public DateTimeOffset? DueDate { get; set; }

        public string Status { get; set; } = "draft";

        public string? Note { get; set; }

        public Guid? CreatedBy { get; set; }

        public string? CreatedByName { get; set; }

        public Guid? ApprovedBy { get; set; }

        public string? ApprovedByName { get; set; }

        public string? SupplierName { get; set; }

        public decimal TotalAmountBeforeTax { get; set; }

        public decimal TotalTaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }
    }
}
