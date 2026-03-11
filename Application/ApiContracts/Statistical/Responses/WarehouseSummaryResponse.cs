using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class WarehouseSummaryResponse
    {
        public int TotalStock { get; set; }

        public decimal TotalValue { get; set; }

        public int LowStockCount { get; set; }

        public int OutOfStockCount { get; set; }
    }
}
