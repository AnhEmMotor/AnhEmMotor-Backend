namespace Application.ApiContracts.PurchaseOrder.Responses
{
    public class PurchaseOrderItemResponse
    {
        public int Id { get; set; }

        public int ProductVariantId { get; set; }

        public string? ProductName { get; set; }

        public int? ProductVariantColorId { get; set; }

        public string? ProductVariantColorName { get; set; }

        public int OrderedQuantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}
