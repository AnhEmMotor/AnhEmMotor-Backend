using System.Text.Json.Serialization;

namespace Application.ApiContracts.PurchaseInvoice.Responses
{
    public class PurchaseInvoiceDetailResponse
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int? PurchaseRequestId { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierPhone { get; set; }
        public string? SupplierAddress { get; set; }
        public string? SupplierTaxCode { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerIdCard { get; set; }
        public DateTimeOffset InvoiceDate { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public string? Notes { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public List<PurchaseInvoiceItemResponse> Items { get; set; } = new();
    }
}
