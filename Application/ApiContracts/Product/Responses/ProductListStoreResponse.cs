namespace Application.ApiContracts.Product.Responses
{
    public class ProductListStoreResponse
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public List<ProductVariantListStoreResponse> Variants { get; set; } = [];
    }
}
