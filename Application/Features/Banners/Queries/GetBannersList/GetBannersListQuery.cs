using Application.ApiContracts.Banner.Responses;
using Application.Common.Models;
using Domain.Primitives;

using MediatR;

namespace Application.Features.Banners.Queries.GetBannersList;

public sealed record GetBannersListQuery : IRequest<Result<PagedResult<BannerResponse>>>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

