using System;

namespace Application.ApiContracts.PurchaseRequest.Responses
{
    public class PurchaseRequestDetailResponse
    {
        public int Id { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Note { get; set; }

        public Guid? CreatedBy { get; set; }

        public string? CreatedByName { get; set; }

        public Guid? ApprovedBy { get; set; }

        public string? ApprovedByName { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public List<PurchaseRequestItemResponse> Items { get; set; } = [];
    }
}
