using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class TopProductRevenueResponse
    {
        public string? ProductName { get; set; }

        public int UnitsSold { get; set; }

        public decimal Revenue { get; set; }
    }
}
