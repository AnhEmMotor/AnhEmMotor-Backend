namespace Application.ApiContracts.Statistical.Responses
{
    public class OrderStatusCountResponse
    {
        public string? StatusName { get; set; }

        public long OrderCount { get; set; }
    }
}
