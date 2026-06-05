namespace Application.ApiContracts.InventoryReceipt.Requests
{
    public class CreateInventoryReceiptInfoRequest
    {
        public int? PurchaseRequestItemId { get; set; }

        public int? SupplierId { get; set; }

        public decimal? UnitPrice { get; set; }

        public int? Count { get; set; }

        public List<VehicleInventoryReceiptRequest>? Vehicles { get; set; }
    }
}
