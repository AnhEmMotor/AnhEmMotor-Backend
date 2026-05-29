namespace Application.ApiContracts.InventoryReceipt.Requests
{
    public class UpdateInputInfoRequest
    {
        public int? Id { get; set; }

        public int? PurchaseRequestItemId { get; set; }

        public int? QuotationProductRowId { get; set; }

        public int? Count { get; set; }

        public List<VehicleInputRequest>? Vehicles { get; set; }
    }
}
