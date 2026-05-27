namespace Application.ApiContracts.Statistical.Responses
{
    public class DashboardStatsResponse
    {
        public decimal TodayRevenue { get; set; }

        public decimal RevenueChangePercentage { get; set; }

        public decimal MonthlyRevenue { get; set; }

        public decimal TodayProfit { get; set; }

        public decimal MonthlyProfit { get; set; }

        public decimal LastMonthRevenue { get; set; }

        public decimal LastMonthProfit { get; set; }

        public decimal Total7dRevenue { get; set; }

        public decimal Total7dProfit { get; set; }

        public decimal BestDayRevenue { get; set; }

        public string? BestDayDate { get; set; }

        public int OverdueOrdersCount { get; set; }

        public int LowStockCount { get; set; }

        public int OverstockCount { get; set; }

        public decimal OverdueDebtAmount { get; set; }

        public int TodayVehiclesSold { get; set; }

        public int MonthlyVehiclesSold { get; set; }

        public int CurrentInventoryCount { get; set; }

        public int TotalSKUCount { get; set; }

        public List<BrandStockResponse> BrandDistribution { get; set; } = [];

        public int ActiveInstallmentCount { get; set; }

        public int LateInstallmentCount { get; set; }

        public decimal TotalCustomerDebt { get; set; }

        public List<TopSellingProductResponse> TopSellingProducts { get; set; } = [];

        public List<BrandRevenueResponse> BrandRevenueDistribution { get; set; } = [];

        public List<string> TodayActivities { get; set; } = [];

        public int PendingOrdersCount { get; set; }

        public int NewCustomersCount { get; set; }
    }
}
