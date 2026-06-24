using System.Text.Json.Serialization;

namespace Application.ApiContracts.Supplier.Responses
{
    public class SupplierResponse
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? StatusId { get; set; }

        public string? Notes { get; set; }

        public string? Address { get; set; }

        public long? TotalInventoryReceipt { get; set; }

        public string? TaxIdentificationNumber { get; set; }

        [JsonPropertyName("partnerTypeId")]
        public string? PartnerTypeId { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonPropertyName("deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
