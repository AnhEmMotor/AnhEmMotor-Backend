using System.Text.Json.Serialization;

namespace Application.ApiContracts.Supplier.Responses
{
    public class SupplierWithTotalInventoryReceiptResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string StatusId { get; set; } = string.Empty;

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        public decimal TotalInventoryReceipt { get; set; }

        public string? Notes { get; set; }

        public string? TaxIdentificationNumber { get; set; }

        [JsonPropertyName("partnerTypeId")]
        public string? PartnerTypeId { get; set; }
    }
}
