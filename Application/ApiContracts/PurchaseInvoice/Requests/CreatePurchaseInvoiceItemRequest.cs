namespace Application.ApiContracts.PurchaseInvoice.Requests
{
    public class CreatePurchaseInvoiceItemRequest
    {
        public int? Id { get; set; }

        public int? PurchaseOrderItemId { get; set; }

        public int? InventoryReceiptItemId { get; set; }


        public int ProductVariantId { get; set; }

        public int? ProductVariantColorId { get; set; }

        public int InvoicedQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TaxRate { get; set; } = 0;
    }
}
