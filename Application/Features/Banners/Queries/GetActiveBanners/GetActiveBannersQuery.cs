using Application.Common.Models;
using MediatR;

namespace Application.Features.Banners.Queries.GetActiveBanners;

public sealed record GetActiveBannersQuery : IRequest<Result<List<BannerResponse>>>;

public sealed record BannerResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string? LinkUrl { get; init; }
    public string? Position { get; init; }
}
