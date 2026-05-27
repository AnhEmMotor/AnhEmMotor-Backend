using Application.ApiContracts.Banner.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Banners.Queries.GetBannersList;

public sealed class GetBannersListQuery : IRequest<Result<List<BannerResponse>>>
{
}

