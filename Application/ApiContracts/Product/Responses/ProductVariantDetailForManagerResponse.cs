using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Responses;

public class ProductVariantDetailForManagerResponse
{
    public int? Id { get; set; }

    [JsonPropertyName("product_id")]
    public int? ProductId { get; set; }

    [JsonPropertyName("url")]
    public string? UrlSlug { get; set; }

    public decimal? Price { get; set; }

    [JsonPropertyName("cover_image_url")]
    public string? CoverImageUrl { get; set; }

    [JsonPropertyName("optionValues")]
    public Dictionary<string, string> OptionValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("photo_collection")]
    public List<string> PhotoCollection { get; set; } = [];

    public int Stock { get; set; }

    [JsonPropertyName("has_been_booked")]
    public int HasBeenBooked { get; set; }

    [JsonPropertyName("status_stock_id")]
    public string StatusStockId { get; set; } = string.Empty;
}

