namespace Application.ApiContracts.PurchaseRequest.Requests
{
    public class CreatePurchaseRequestItemRequest
    {
        public int? ProductVariantId { get; set; }

        public int? ProductVariantColorId { get; set; }

        public int? Quantity { get; set; }

        public int? SupplierId { get; set; }
    }
}
