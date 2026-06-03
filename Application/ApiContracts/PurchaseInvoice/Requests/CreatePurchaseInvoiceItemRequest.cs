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

        public System.Collections.Generic.List<VehicleInvoiceRequest>? Vehicles { get; set; }
    }

    public class VehicleInvoiceRequest
    {
        public int? Id { get; set; }
        public string VinNumber { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public decimal ImportPrice { get; set; }
    }
}
