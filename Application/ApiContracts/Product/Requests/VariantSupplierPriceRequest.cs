using Application.Common.Converters;
using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Requests;

public class VariantSupplierPriceRequest
{
    [JsonPropertyName("supplier_id")]
    public int SupplierId { get; set; }

    [JsonPropertyName("product_variant_color_id")]
    public int? ProductVariantColorId { get; set; }

    [JsonPropertyName("quote_price")]
    [JsonConverter(typeof(NullableDecimalConverter))]
    public decimal? QuotePrice { get; set; }

    public string? Note { get; set; }
}
