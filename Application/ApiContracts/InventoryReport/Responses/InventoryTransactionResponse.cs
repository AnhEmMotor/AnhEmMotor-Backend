using System;

namespace Application.ApiContracts.InventoryReport.Responses
{
    public class InventoryTransactionResponse
    {
        public int Id { get; set; }

        public string? PartnerName { get; set; }

        public int Qty { get; set; }

        public decimal Price { get; set; }

        public DateTimeOffset Date { get; set; }
    }
}
