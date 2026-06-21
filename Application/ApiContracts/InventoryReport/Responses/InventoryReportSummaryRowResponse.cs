namespace Application.ApiContracts.InventoryReport.Responses
{
    public class InventoryReportSummaryRowResponse
    {
        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public int VariantId { get; set; }

        public string? VariantName { get; set; }

        public int? ColorId { get; set; }

        public string? ColorName { get; set; }

        public int BeginningQty { get; set; }

        public int ImportedQty { get; set; }

        public int ExportedQty { get; set; }

        public int StockQty { get; set; }

        public int OrderedQty { get; set; }
    }
}
