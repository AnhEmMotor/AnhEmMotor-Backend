using System;

namespace Application.ApiContracts.PurchaseRequest.Responses
{
    public class PurchaseRequestListResponse
    {
        public int Id { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Note { get; set; }

        public string? CreatedByName { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public int TotalItems { get; set; }
    }
}
