using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class DailyRevenueTableResponse
    {
        public DateOnly ReportDay { get; set; }

        public int OrdersCount { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal TotalProfit { get; set; }

        public double Growth { get; set; }

        public bool HasZeroCostPrice { get; set; }
    }
}
