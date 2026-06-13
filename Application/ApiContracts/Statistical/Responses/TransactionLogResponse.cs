using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Statistical.Responses
{
    public class TransactionLogResponse
    {
        public DateTime Timestamp { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsRevenue { get; set; } // True: Thu, False: Chi
        public string Status { get; set; } = string.Empty; // Completed, Pending, Refund
        public string StaffName { get; set; } = string.Empty;
    }
}
