namespace Application.ApiContracts.PurchaseInvoice.Responses
{
    public class PurchaseInvoiceItemResponse
    {
        public int Id { get; set; }

        public int PurchaseInvoiceId { get; set; }

        public int? PurchaseOrderItemId { get; set; }

        public int? InventoryReceiptItemId { get; set; }

        public int ProductVariantId { get; set; }

        public string? ProductVariantName { get; set; }

        public string? SKU { get; set; }

        public int? ProductVariantColorId { get; set; }

        public string? ColorName { get; set; }

        public int InvoicedQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TaxRate { get; set; }

        public decimal TotalPrice => InvoicedQuantity * UnitPrice;

        public decimal TaxAmount => TotalPrice * (TaxRate / 100);

        public decimal TotalAmount => TotalPrice + TaxAmount;
    }
}
