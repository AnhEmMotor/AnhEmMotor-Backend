namespace Application.ApiContracts.Input.Requests
{
    public class CreateInputInfoRequest
    {
        public int? ProductVariantId { get; set; }

        public int? ProductVariantColorId { get; set; }

        public int? Count { get; set; }

        public decimal? InputPrice { get; set; }

        public List<VehicleInputRequest>? Vehicles { get; set; }
    }
}
