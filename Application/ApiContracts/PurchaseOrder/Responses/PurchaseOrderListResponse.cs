using System;

namespace Application.ApiContracts.PurchaseOrder.Responses
{
    public class PurchaseOrderListResponse
    {
        public int Id { get; set; }

        public int? PurchaseRequestId { get; set; }

        public int SupplierId { get; set; }

        public string? SupplierName { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTimeOffset OrderDate { get; set; }

        public string? Note { get; set; }

        public string? CreatedByName { get; set; }

        public string? SentByName { get; set; }

        public string? ApprovedByName { get; set; }

        public string? RejectedByName { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
