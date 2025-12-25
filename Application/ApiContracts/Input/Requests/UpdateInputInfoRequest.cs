namespace Application.ApiContracts.Input.Requests
{
    public class UpdateInputInfoRequest
    {
        public int? Id { get; set; }

        public int? ProductId { get; set; }

        public int? Count { get; set; }

        public decimal? InputPrice { get; set; }
    }
}
