using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Create;

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

    /// <summary>
    /// Dictionary mapping option names to their values (e.g., {"Color": "Red", "Size": "Large"})
    /// Used when creating products with string-based option values
    /// </summary>
    [JsonPropertyName("optionValues")]
    public Dictionary<string, string> OptionValues { get; set; } = [];

    /// <summary>
    /// List of option value IDs for backward compatibility
    /// Used internally for database lookups
    /// </summary>
    [JsonIgnore]
    public List<int> OptionValueIds { get; set; } = [];
}
