using Application.Common.Converters;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Requests;

public class CreateProductVariantRequest
{
    [JsonPropertyName("price")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? Price { get; set; }

    public string? UrlSlug { get; set; }

    [JsonPropertyName("cover_image_url")]
    public string? CoverImageUrl { get; set; }

    [JsonPropertyName("photo_collection")]
    public List<string> PhotoCollection { get; set; } = [];

    [JsonPropertyName("optionValues")]
    public Dictionary<string, string> OptionValues { get; set; } = [];

    [JsonPropertyName("version_name")]
    public string? VersionName { get; set; }

    [JsonPropertyName("color_name")]
    public string? ColorName { get; set; }

    [JsonPropertyName("color_code")]
    public string? ColorCode { get; set; }

    [JsonPropertyName("sku")]
    public string? SKU { get; set; }

    [JsonIgnore]
    public List<int> OptionValueIds { get; set; } = [];
}
