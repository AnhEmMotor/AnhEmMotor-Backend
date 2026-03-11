using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class BrandRevenueResponse
    {
        public string? BrandName { get; set; }

        public decimal Revenue { get; set; }
    }
}
