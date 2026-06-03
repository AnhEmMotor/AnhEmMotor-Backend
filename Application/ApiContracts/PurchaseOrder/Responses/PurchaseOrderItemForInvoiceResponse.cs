namespace Application.ApiContracts.PurchaseOrder.Responses
{
    public class PurchaseOrderItemForInvoiceResponse
    {
        public int Id { get; set; }

        public int ProductVariantId { get; set; }

        public string? ProductName { get; set; }

        public int? ProductVariantColorId { get; set; }

        public string? ProductVariantColorName { get; set; }

        public int OrderedQuantity { get; set; }

        public int InvoicedQuantity { get; set; }

        public int InvoicingQuantity { get; set; }

        public int RemainingQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public bool NeedVin { get; set; }

        public int? QuotationProductRowId { get; set; }

        public int? PurchaseRequestItemId { get; set; }

        public int? QuotationId { get; set; }

        public string? QuotationName { get; set; }

        public System.Collections.Generic.List<PurchaseOrderVehicleInvoiceResponse> ImportedVehicles { get; set; } = [];
    }
}
