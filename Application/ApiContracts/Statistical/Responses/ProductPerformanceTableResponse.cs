using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class ProductPerformanceTableResponse
    {
        public string? ProductName { get; set; }

        public decimal SellPrice { get; set; }

        public int SoldCount30Days { get; set; }

        public int StockQuantity { get; set; }

        public int MaxStockQuantity { get; set; }

        public double MarginPercentage { get; set; }

        public string? Status { get; set; }

        public int[] Trend { get; set; } = [];
    }
}
