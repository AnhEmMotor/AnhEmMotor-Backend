using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class TopProductProfitResponse
    {
        public string? ProductName { get; set; }

        public decimal Profit { get; set; }
    }
}
