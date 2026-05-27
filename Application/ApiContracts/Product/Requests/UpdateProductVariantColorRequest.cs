using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Requests;

public class UpdateProductVariantColorRequest
{
    public int? Id { get; set; }

    [JsonPropertyName("color_name")]
    public string? ColorName { get; set; }

    [JsonPropertyName("color_code")]
    public string? ColorCode { get; set; }

    [JsonPropertyName("cover_image_url")]
    public string? CoverImageUrl { get; set; }
}
