using MediatR;
using Application.Common.Models;
using System.Text.Json.Serialization;
using Sieve.Models;
using Domain.Primitives;

namespace Application.Features.News.Queries.GetProductsForNews;

public class GetProductsForNewsQuery : IRequest<Result<PagedResult<ProductNewsDto>>>
{
    public SieveModel? SieveModel { get; init; }
}

public class ProductNewsDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("img")]
    public string Img { get; set; } = string.Empty;

    [JsonPropertyName("colors")]
    public List<ProductNewsColorDto> Colors { get; set; } = new();
}

public class ProductNewsColorDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("img")]
    public string Img { get; set; } = string.Empty;
}
