namespace Application.ApiContracts.PurchaseOrder.Requests
{
    public class CreatePurchaseOrderItemRequest
    {
        public int? ProductVariantId { get; set; }

        public int? ProductVariantColorId { get; set; }

        public int? OrderedQuantity { get; set; }

        public decimal? UnitPrice { get; set; }

        public int? SupplierId { get; set; }
    }
}
