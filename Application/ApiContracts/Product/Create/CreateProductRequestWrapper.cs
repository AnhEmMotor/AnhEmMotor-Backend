using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Create;

public class CreateProductRequestWrapper
{
    [JsonPropertyName("p_product_data")]
    public UpsertProductRequest? ProductData { get; set; }
}
