using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Requests;

public class UpdateProductVariantRequest
{
    public int? Id { get; set; }

    public decimal? Price { get; set; }

    [JsonPropertyName("url")]
    public string? UrlSlug { get; set; }

    [JsonPropertyName("cover_image_url")]
    public string? CoverImageUrl { get; set; }

    [JsonPropertyName("photo_collection")]
    public List<string> PhotoCollection { get; set; } = [];

    [JsonPropertyName("optionValues")]
    public Dictionary<string, string> OptionValues { get; set; } = [];

    [JsonIgnore]
    public List<int> OptionValueIds { get; set; } = [];
}
