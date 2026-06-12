using System;

namespace Application.ApiContracts.InventoryReport.Responses
{
    public class InventoryReportDetailResponse
    {
        public List<InventoryTransactionResponse> Imports { get; set; } = [];

        public List<InventoryTransactionResponse> Exports { get; set; } = [];
    }
}
