using System.Collections.Generic;

namespace Application.ApiContracts.InventoryReport.Responses
{
    public class InventoryReportSummaryResponse
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int ImportedQty { get; set; }
        public int ExportedQty { get; set; }
        public int InventoryQty { get; set; }
        public int OrderedQty { get; set; }
        public int RemainingQty { get; set; }
        public List<InventoryReportVariantResponse> Variants { get; set; } = [];
    }

    public class InventoryReportVariantResponse
    {
        public int VariantId { get; set; }
        public string? VariantName { get; set; }
        public int ImportedQty { get; set; }
        public int ExportedQty { get; set; }
        public int InventoryQty { get; set; }
        public int OrderedQty { get; set; }
        public int RemainingQty { get; set; }
        public List<InventoryReportColorResponse>? VariantColors { get; set; }
    }

    public class InventoryReportColorResponse
    {
        public int ColorId { get; set; }
        public string? ColorName { get; set; }
        public int ImportedQty { get; set; }
        public int ExportedQty { get; set; }
        public int InventoryQty { get; set; }
        public int OrderedQty { get; set; }
        public int RemainingQty { get; set; }
    }
}

