namespace Application.ApiContracts.Supplier.Requests
{
    public class RestoreManySuppliersRequest
    {
        public List<int> Ids { get; set; } = [];
    }
}
