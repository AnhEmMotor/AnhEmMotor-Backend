using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Responses
{
    public class ProductTechnologyResponse
    {
        [JsonPropertyName("technology_id")]
        public int TechnologyId { get; set; }

        [JsonPropertyName("custom_title")]
        public string? CustomTitle { get; set; }

        [JsonPropertyName("custom_description")]
        public string? CustomDescription { get; set; }

        [JsonPropertyName("custom_image_url")]
        public string? CustomImageUrl { get; set; }

        [JsonPropertyName("display_order")]
        public int DisplayOrder { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("_default_title")]
        public string? DefaultTitle { get; set; }

        [JsonPropertyName("_default_description")]
        public string? DefaultDescription { get; set; }

        [JsonPropertyName("_default_image_url")]
        public string? DefaultImageUrl { get; set; }

        [JsonPropertyName("_category_name")]
        public string? CategoryName { get; set; }
    }
}
