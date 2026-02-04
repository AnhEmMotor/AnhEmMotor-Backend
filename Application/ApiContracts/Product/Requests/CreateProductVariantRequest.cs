using System.Text.Json.Serialization;

namespace Application.ApiContracts.Product.Requests;

public class CreateProductVariantRequest
{
    public decimal? Price { get; set; }
    
    public string? UrlSlug { get; set; }

    public string? CoverImageUrl { get; set; }

    public List<string> PhotoCollection { get; set; } = [];

    public Dictionary<string, string> OptionValues { get; set; } = [];

    [JsonIgnore]
    public List<int> OptionValueIds { get; set; } = [];
}
