
namespace Application.ApiContracts.Banner.Responses;

public sealed record BannerResponse
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string ImageUrl { get; init; } = string.Empty;

    public string? LinkUrl { get; init; }

    public string? Position { get; init; }

    public int DisplayOrder { get; init; }
}
