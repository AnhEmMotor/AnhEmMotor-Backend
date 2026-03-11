using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class StockStatusRatioResponse
    {
        public string? StatusLabel { get; set; }

        public int Count { get; set; }
    }
}
