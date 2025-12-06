namespace Application.ApiContracts.ProductCategory.Requests
{
    public class DeleteManyProductCategoriesRequest
    {
        public List<int> Ids { get; set; } = [];
    }
}
