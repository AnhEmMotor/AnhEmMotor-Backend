using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class ExpenseDetailResponse
    {
        public string Category { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }
}
