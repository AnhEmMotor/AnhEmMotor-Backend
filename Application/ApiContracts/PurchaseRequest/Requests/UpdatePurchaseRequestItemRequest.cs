namespace Application.ApiContracts.PurchaseRequest.Requests
{
    public class UpdatePurchaseRequestItemRequest
    {
        public int? Id { get; set; }

        public int? ProductVariantId { get; set; }

        public int? ProductVariantColorId { get; set; }

        public int? Quantity { get; set; }
    }
}
