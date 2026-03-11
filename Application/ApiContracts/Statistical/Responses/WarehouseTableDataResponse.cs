using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class WarehouseTableDataResponse
    {
        public string? BrandName { get; set; }

        public int TotalStock { get; set; }

        public int Capacity { get; set; }

        public int LowStock { get; set; }

        public int OutOfStock { get; set; }

        public string? Status { get; set; }

        public decimal Value { get; set; }
    }
}
