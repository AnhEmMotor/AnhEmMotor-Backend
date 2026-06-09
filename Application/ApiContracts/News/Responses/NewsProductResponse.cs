using System.Text.Json.Serialization;

namespace Application.ApiContracts.News.Responses;

public sealed record NewsProductResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; init; } = string.Empty;

    [JsonPropertyName("img")]
    public string Img { get; init; } = string.Empty;

    [JsonPropertyName("productVariantId")]
    public int ProductVariantId { get; init; }

    [JsonPropertyName("productVariantColorId")]
    public int? ProductVariantColorId { get; init; }

    [JsonPropertyName("variantName")]
    public string VariantName { get; init; } = string.Empty;

    [JsonPropertyName("colorName")]
    public string? ColorName { get; init; }
}
