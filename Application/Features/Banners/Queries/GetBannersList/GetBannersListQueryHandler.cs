using Application.ApiContracts.Banner.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Banner;
using Mapster;
using MediatR;
using System;

namespace Application.Features.Banners.Queries.GetBannersList
{
    public class GetBannersListQueryHandler(IBannerReadRepository bannerRepository) : IRequestHandler<GetBannersListQuery, Result<List<BannerResponse>>>
    {
        public async Task<Result<List<BannerResponse>>> Handle(
            GetBannersListQuery request,
            CancellationToken cancellationToken)
        {
            var banners = await bannerRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var response = banners.Adapt<List<BannerResponse>>().OrderBy(b => b.DisplayOrder).ToList();
            return Result<List<BannerResponse>>.Success(response);
        }
    }
}
