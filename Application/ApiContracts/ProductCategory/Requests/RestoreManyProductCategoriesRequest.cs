namespace Application.ApiContracts.ProductCategory.Requests
{
    public class RestoreManyProductCategoriesRequest
    {
        public List<int> Ids { get; set; } = [];
    }
}
