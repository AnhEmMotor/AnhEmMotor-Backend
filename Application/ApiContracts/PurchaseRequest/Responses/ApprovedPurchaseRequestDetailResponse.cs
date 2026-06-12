using System;

namespace Application.ApiContracts.PurchaseRequest.Responses
{
    public class ApprovedPurchaseRequestDetailResponse
    {
        public int Id { get; set; }

        public string? Note { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public List<ApprovedPurchaseRequestItemResponse> Items { get; set; } = [];
    }
}
