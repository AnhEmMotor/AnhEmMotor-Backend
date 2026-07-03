namespace Application.ApiContracts.PurchaseInvoice.Responses
{
    public class PurchaseInvoiceItemResponse
    {
        public int Id { get; set; }
        public int ProductVariantId { get; set; }
        public string? ProductName { get; set; }
        public string? VariantName { get; set; }
        public int? ProductVariantColorId { get; set; }
        public string? ColorName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
