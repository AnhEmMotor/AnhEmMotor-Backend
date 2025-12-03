namespace Application.ApiContracts.Output
{
    public class UpdateOutputInfoRequest
    {
        public int? Id { get; set; }

        public int? ProductId { get; set; }

        public short? Count { get; set; }
    }
}
