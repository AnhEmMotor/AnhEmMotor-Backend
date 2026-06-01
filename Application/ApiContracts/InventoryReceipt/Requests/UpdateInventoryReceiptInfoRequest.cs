namespace Application.ApiContracts.InventoryReceipt.Requests
{
    public class UpdateInventoryReceiptInfoRequest
    {
        public int? Id { get; set; }

        public int? PurchaseOrderItemId { get; set; }

        public int? Count { get; set; }

        public List<VehicleInventoryReceiptRequest>? Vehicles { get; set; }
    }
}
