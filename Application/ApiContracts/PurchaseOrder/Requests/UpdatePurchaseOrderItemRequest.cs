namespace Application.ApiContracts.PurchaseOrder.Requests
{
    public class UpdatePurchaseOrderItemRequest
    {
        public int? Id { get; set; }

        public int? ProductVariantId { get; set; }

        public int? ProductVariantColorId { get; set; }

        public int? OrderedQuantity { get; set; }

        public decimal? UnitPrice { get; set; }

        public int? QuotationProductRowId { get; set; }
    }
}
