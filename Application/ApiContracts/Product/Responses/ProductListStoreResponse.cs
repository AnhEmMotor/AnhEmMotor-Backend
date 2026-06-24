namespace Application.ApiContracts.Product.Responses
{
    public class ProductListStoreResponse
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int? CategoryId { get; set; }

        public string? Category { get; set; }

        public int? BrandId { get; set; }

        public string? Brand { get; set; }

        public decimal? Displacement { get; set; }

        public int HasBeenBooked { get; set; }

        public List<ProductVariantListStoreResponse> Variants { get; set; } = [];
    }
}
