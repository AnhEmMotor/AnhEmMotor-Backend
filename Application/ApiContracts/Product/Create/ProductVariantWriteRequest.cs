using System;

namespace Application.ApiContracts.Product.Create;

public class ProductVariantWriteRequest
{
    public int? Id { get; set; }
    public long? Price { get; set; }
    public string? UrlSlug { get; set; }
    public string? CoverImageUrl { get; set; }
    public List<string> PhotoCollection { get; set; } = [];
    public List<int> OptionValueIds { get; set; } = [];
}
