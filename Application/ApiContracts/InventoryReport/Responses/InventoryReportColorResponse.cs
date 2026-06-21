using System;

namespace Application.ApiContracts.InventoryReport.Responses
{
    public class InventoryReportColorResponse
    {
        public int ColorId { get; set; }

        public string? ColorName { get; set; }

        public int BeginningQty { get; set; }

        public int ImportedQty { get; set; }

        public int ExportedQty { get; set; }

        public int InventoryQty { get; set; }

        public int OrderedQty { get; set; }

        public int RemainingQty { get; set; }
    }
}
