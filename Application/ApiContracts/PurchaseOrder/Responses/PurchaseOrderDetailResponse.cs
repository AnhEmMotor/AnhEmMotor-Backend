using System;
using System.Collections.Generic;

namespace Application.ApiContracts.PurchaseOrder.Responses
{
    public class PurchaseOrderDetailResponse
    {
        public int Id { get; set; }

        public int? PurchaseRequestId { get; set; }

        public int SupplierId { get; set; }

        public string? SupplierName { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTimeOffset OrderDate { get; set; }

        public string? Note { get; set; }

        public Guid? CreatedBy { get; set; }

        public string? CreatedByName { get; set; }

        public Guid? SentBy { get; set; }

        public string? SentByName { get; set; }

        public Guid? ApprovedBy { get; set; }

        public string? ApprovedByName { get; set; }

        public Guid? RejectedBy { get; set; }

        public string? RejectedByName { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public List<PurchaseOrderItemResponse> Items { get; set; } = [];
    }
}
