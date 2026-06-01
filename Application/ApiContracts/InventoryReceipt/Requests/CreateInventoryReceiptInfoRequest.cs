namespace Application.ApiContracts.InventoryReceipt.Requests
{
    public class CreateInventoryReceiptInfoRequest
    {
        public int? PurchaseOrderItemId { get; set; }

        public int? Count { get; set; }

        public List<VehicleInventoryReceiptRequest>? Vehicles { get; set; }
    }
}
