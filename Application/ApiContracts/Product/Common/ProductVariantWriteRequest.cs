using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product;

public class ProductVariantWriteRequest
{
    public int? Id { get; set; }
    public long? Price { get; set; }

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
