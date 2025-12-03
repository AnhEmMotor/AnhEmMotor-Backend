namespace Application.ApiContracts.Staticals
{
    public class ProductReportResponse
    {
        public string? ProductName { get; set; }

        public int VariantId { get; set; }

        public long StockQuantity { get; set; }

        public long SoldLastMonth { get; set; }
    }
}
