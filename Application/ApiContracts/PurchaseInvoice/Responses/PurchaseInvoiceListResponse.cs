using System.Text.Json.Serialization;

namespace Application.ApiContracts.PurchaseInvoice.Responses
{
    public class PurchaseInvoiceListResponse
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTimeOffset InvoiceDate { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? SupplierName { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public string? Notes { get; set; }
        public int TotalItems { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        [JsonPropertyName("deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
