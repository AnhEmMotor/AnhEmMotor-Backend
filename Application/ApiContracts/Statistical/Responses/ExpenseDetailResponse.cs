using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Statistical.Responses
{
    public class ExpenseDetailResponse
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
