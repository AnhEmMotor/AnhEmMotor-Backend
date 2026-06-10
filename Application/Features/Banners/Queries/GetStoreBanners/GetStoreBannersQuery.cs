using Application.ApiContracts.Banner.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Banners.Queries.GetStoreBanners;

public sealed record GetStoreBannersQuery : IRequest<Result<List<BannerResponse>>>
{
    public string? Placement { get; init; }
}
