using System;

namespace Application.ApiContracts.InventoryLedger.Responses
{
    public class InventoryLedgerResponse
    {
        public int Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // 'IMPORT', 'EXPORT', 'ADJUST'
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string? ColorName { get; set; }
        public string? Partner { get; set; }
        public int ImportQty { get; set; }
        public int ExportQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public int Balance { get; set; }
    }
}
