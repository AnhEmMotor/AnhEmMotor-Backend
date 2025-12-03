namespace Application.ApiContracts.Input
{
    public class CreateInputInfoRequest
    {
        public int? ProductId { get; set; }

        public short? Count { get; set; }

        public long? InputPrice { get; set; }
    }
}
