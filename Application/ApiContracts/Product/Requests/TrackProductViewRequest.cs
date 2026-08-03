namespace Application.ApiContracts.Product.Requests
{
    public class TrackProductViewRequest
    {
        public int DwellTimeMs { get; set; }

        public string? VisitorKey { get; set; }
    }
}
