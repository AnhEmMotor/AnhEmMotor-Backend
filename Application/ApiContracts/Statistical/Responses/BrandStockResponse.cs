using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class BrandStockResponse
    {
        public string? BrandName { get; set; }

        public int InStock { get; set; }

        public int LowStock { get; set; }

        public int OutOfStock { get; set; }
    }
}
