namespace Application.ApiContracts.PurchaseInvoice.Requests
{
    public class CreatePurchaseInvoiceItemRequest
    {
        public int? PurchaseRequestItemId { get; set; }
        public int ProductVariantId { get; set; }
        public int? ProductVariantColorId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
    }

    public class CreatePurchaseInvoiceRequest
    {
        public int? PurchaseRequestId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTimeOffset InvoiceDate { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierPhone { get; set; }
        public string? SupplierAddress { get; set; }
        public string? SupplierTaxCode { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerIdCard { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseInvoiceItemRequest> Items { get; set; } = new();
    }
}
