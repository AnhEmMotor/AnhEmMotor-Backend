namespace Application.ApiContracts.Output.Requests
{
    public class UpdateOutputInfoRequest
    {
        public int? Id { get; set; }

        public int? ProductId { get; set; }

        public int? Count { get; set; }
    }
}
