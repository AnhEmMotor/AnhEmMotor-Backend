namespace Application.ApiContracts.Statistical.Responses
{
    public class DashboardStatsResponse
    {
        // 1. FINANCIALS (💰 Tiền có khỏe không?)
        public decimal TodayRevenue { get; set; }
        public decimal RevenueChangePercentage { get; set; } // vs Yesterday
        public decimal MonthlyRevenue { get; set; }
        public decimal TodayProfit { get; set; }
        public decimal MonthlyProfit { get; set; }
        public decimal LastMonthRevenue { get; set; }
        public decimal LastMonthProfit { get; set; }

        // 7-Day Insights for Chart
        public decimal Total7dRevenue { get; set; }
        public decimal Total7dProfit { get; set; }
        public decimal BestDayRevenue { get; set; }
        public string? BestDayDate { get; set; }

        // 2. OPERATIONS & ALERTS (🚨 Vấn đề cần xử lý ngay?)
        public int OverdueOrdersCount { get; set; } // Pending > 2 hours
        public int LowStockCount { get; set; } // Units < 3
        public int OverstockCount { get; set; } // > 60 days
        public decimal OverdueDebtAmount { get; set; } // Proxy calculation

        // 3. KHO (📦 Kho có ổn không?)
        public int TodayVehiclesSold { get; set; }
        public int MonthlyVehiclesSold { get; set; }
        public int CurrentInventoryCount { get; set; }
        public int TotalSKUCount { get; set; }
        public List<BrandStockResponse> BrandDistribution { get; set; } = [];

        // 4. CÔNG NỢ & TRẢ GÓP (📈 Hôm nay bán được không?)
        public int ActiveInstallmentCount { get; set; }
        public int LateInstallmentCount { get; set; } // Proxy calculation
        public decimal TotalCustomerDebt { get; set; }

        // 5. INSIGHTS (🔥 Chiều sâu thông tin)
        public List<TopSellingProductResponse> TopSellingProducts { get; set; } = [];
        public List<BrandRevenueResponse> BrandRevenueDistribution { get; set; } = [];
        public List<string> TodayActivities { get; set; } = []; // ["3 xe đã bán", "2 đơn trả góp", "1 khách mới"]

        // Mappings for old fields to maintain compatibility
        public int PendingOrdersCount { get; set; }
        public int NewCustomersCount { get; set; }
    }
}
