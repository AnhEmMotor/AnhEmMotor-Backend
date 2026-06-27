using System;

namespace Application.ApiContracts.InventoryReport.Responses
{
    public class InventoryReportVariantResponse
    {
        public int VariantId { get; set; }

        public string? VariantName { get; set; }

        public int BeginningQty { get; set; }

        public int ImportedQty { get; set; }

        public int ExportedQty { get; set; }

        public int InventoryQty { get; set; }

        public List<InventoryReportColorResponse>? VariantColors { get; set; }
    }
}
