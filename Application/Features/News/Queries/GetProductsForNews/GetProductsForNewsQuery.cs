using MediatR;
using Application.Common.Models;
using System.Text.Json.Serialization;

namespace Application.Features.News.Queries.GetProductsForNews;

public class GetProductsForNewsQuery : IRequest<Result<List<ProductNewsDto>>>
{
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
}
