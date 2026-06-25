using System;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.PurchaseRequest.Responses
{
    public class PurchaseRequestListResponse
    {
        public int Id { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Note { get; set; }

        public string? CreatedByName { get; set; }

        public string? SentByName { get; set; }

        public string? ApprovedByName { get; set; }

        public string? RejectedByName { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public int TotalItems { get; set; }

        public bool IsFullyImported { get; set; }

        [JsonPropertyName("deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
