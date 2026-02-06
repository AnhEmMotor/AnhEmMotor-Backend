namespace Application.ApiContracts.Supplier.Responses
{
    public sealed class SupplierForInputManagerResponse
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Notes { get; set; }

        public string? Address { get; set; }

        public long? TotalInput { get; set; }

        public string? TaxIdentificationNumber { get; set; }
    }
}
