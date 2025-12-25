namespace Application.ApiContracts.Statistical.Responses
{
    public class ProductReportResponse
    {
        public string? ProductName { get; set; }

        public int VariantId { get; set; }

        public int StockQuantity { get; set; }

        public int SoldLastMonth { get; set; }
    }
}
