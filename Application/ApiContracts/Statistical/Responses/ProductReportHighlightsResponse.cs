using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class ProductReportHighlightsResponse
    {
        public string? BestSellerName { get; set; }

        public int BestSellerSold { get; set; }

        public string? DeadStockName { get; set; }

        public decimal DeadStockValue { get; set; }

        public double AvgTurnover { get; set; }

        public int TotalSKUs { get; set; }
    }
}
