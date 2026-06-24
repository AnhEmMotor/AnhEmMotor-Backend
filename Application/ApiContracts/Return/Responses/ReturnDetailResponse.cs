using System;

namespace Application.ApiContracts.Return.Responses
{
    public class ReturnDetailResponse
    {
        public int Id { get; set; }

        public string TrackingNumber { get; set; } = string.Empty;

        public string OriginalTrackingNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string Carrier { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string? BoxCondition { get; set; }

        public string? ProductCondition { get; set; }

        public string? ReturnProofImage { get; set; }

        public string? ReturnInternalNote { get; set; }

        public string? ReturnAction { get; set; }

        public List<ReturnDetailItemResponse> Items { get; set; } = [];
    }
}
