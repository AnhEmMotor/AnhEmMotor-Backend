using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Responses
{
    public class ProductListStoreResponse
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public List<ProductVariantListStoreResponse> Variants { get; set; } = [];
    }

    public class ProductVariantListStoreResponse
    {
        public int Id { get; set; }

        [JsonPropertyName("url")]
        public string? UrlSlug { get; set; }

        public decimal? Price { get; set; }

        [JsonPropertyName("cover_image_url")]
        public string? CoverImageUrl { get; set; }

        [JsonPropertyName("propertyName")]
        public string? OptionValuesText { get; set; }
    }
}
