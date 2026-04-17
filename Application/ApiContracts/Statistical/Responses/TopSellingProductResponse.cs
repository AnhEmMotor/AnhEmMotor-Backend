namespace Application.ApiContracts.Statistical.Responses
{
    public class TopSellingProductResponse
    {
        public string? ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
