namespace Application.ApiContracts.Supplier.Requests
{
    public class UpdateManySupplierStatusRequest
    {
        public List<int> Ids { get; set; } = [];

        public string? StatusId { get; set; }
    }
}
