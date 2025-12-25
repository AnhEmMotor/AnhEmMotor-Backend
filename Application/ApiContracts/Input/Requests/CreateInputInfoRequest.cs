namespace Application.ApiContracts.Input.Requests
{
    public class CreateInputInfoRequest
    {
        public int? ProductId { get; set; }

        public int? Count { get; set; }

        public decimal? InputPrice { get; set; }
    }
}
