using System;
using System.Collections.Generic;

namespace Application.ApiContracts.PurchaseRequest.Responses
{
    public class ApprovedPurchaseRequestDetailResponse
    {
        public int Id { get; set; }

        public string? Note { get; set; }

        public Guid? CreatedBy { get; set; }

        public string? CreatedByName { get; set; }

        public Guid? ApprovedBy { get; set; }

        public string? ApprovedByName { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public List<ApprovedPurchaseRequestItemResponse> Items { get; set; } = [];
    }
}
