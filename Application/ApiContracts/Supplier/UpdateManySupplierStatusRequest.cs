namespace Application.ApiContracts.Supplier
{
    public class UpdateManySupplierStatusRequest
    {
        public List<int> Ids { get; set; } = [];

        public string? StatusId { get; set; }
    }
}
