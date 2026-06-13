namespace Application.ApiContracts.Supplier.Responses
{
    public class SupplierForInventoryReceiptManagerResponse
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Notes { get; set; }

        public string? Address { get; set; }

        public long? TotalInventoryReceipt { get; set; }

        public string? TaxIdentificationNumber { get; set; }
    }
}
