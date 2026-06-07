using System;
using System.Collections.Generic;

namespace Application.ApiContracts.InventoryReport.Responses
{
    public class InventoryReportDetailResponse
    {
        public List<InventoryTransactionResponse> Imports { get; set; } = [];
        public List<InventoryTransactionResponse> Exports { get; set; } = [];
    }

    public class InventoryTransactionResponse
    {
        public int Id { get; set; }
        public string? PartnerName { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}
