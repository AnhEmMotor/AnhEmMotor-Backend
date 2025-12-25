namespace Application.ApiContracts.Supplier.Responses
{
    public sealed class SupplierWithTotalInputResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string StatusId { get; set; } = string.Empty;

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        public decimal TotalInput { get; set; }
    }
}
