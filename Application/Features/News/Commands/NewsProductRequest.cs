using System.Text.Json.Serialization;

namespace Application.Features.News.Commands;

public sealed record NewsProductRequest
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; init; } = string.Empty;

    [JsonPropertyName("img")]
    public string Img { get; init; } = string.Empty;
}
