namespace Application.ApiContracts.PurchaseRequest.Responses
{
    public class PurchaseRequestItemResponse
    {
        public int Id { get; set; }

        public int ProductVariantId { get; set; }

        public string? ProductName { get; set; }

        public int? ProductVariantColorId { get; set; }

        public string? ProductVariantColorName { get; set; }

        public int Quantity { get; set; }

        public int POCreatingQuantity { get; set; }

        public int POApprovedQuantity { get; set; }

        public int PORemainingQuantity { get; set; }

        public int ImportedQuantity { get; set; }

        public int PendingQuantity { get; set; }

        public int UnimportedQuantity { get; set; }
    }
}
