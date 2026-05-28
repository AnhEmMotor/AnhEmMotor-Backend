namespace Application.ApiContracts.Input.Requests
{
    public class CreateInputInfoRequest
    {
        public int? PurchaseRequestItemId { get; set; }

        public int? QuotationProductRowId { get; set; }

        public int? Count { get; set; }

        public List<VehicleInputRequest>? Vehicles { get; set; }
    }
}
