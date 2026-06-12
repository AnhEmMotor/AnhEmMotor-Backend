namespace Application.ApiContracts.PurchaseRequest.Responses
{
    public class ApprovedPurchaseRequestItemResponse
    {
        public int Id { get; set; }

        public int ProductVariantId { get; set; }

        public string? ProductName { get; set; }

        public int? ProductVariantColorId { get; set; }

        public string? ProductVariantColorName { get; set; }

        public int Quantity { get; set; }

        public int UnimportedQuantity { get; set; }

        public int InvoicedQuantity { get; set; }

        public int InvoicingQuantity { get; set; }

        public int UninvoicedQuantity { get; set; }

        public bool NeedVin { get; set; }
    }
}
