namespace Application.ApiContracts.Supplier
{
    public class CreateSupplierRequest
    {
        public string? Name { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Notes { get; set; }

        public string? Address { get; set; }
    }
}
