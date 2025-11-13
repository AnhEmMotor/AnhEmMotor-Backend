namespace Application.ApiContracts.Supplier
{
    public class SupplierResponse
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? StatusId { get; set; }
        public string? Notes { get; set; }
        public string? Address { get; set; }
    }
}
