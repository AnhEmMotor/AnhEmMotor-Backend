using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Statistical.Responses
{
        public class StaffPerformanceResponse
        {
            public string EmployeeName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public decimal TotalSales { get; set; }
            public decimal TargetSales { get; set; }
            public decimal CommissionPaid { get; set; }
            public string KpiStatus { get; set; } = string.Empty; // Vượt KPI, Đạt, Cần cải thiện
            public bool IsTopSeller { get; set; }
        }
}
