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
}

